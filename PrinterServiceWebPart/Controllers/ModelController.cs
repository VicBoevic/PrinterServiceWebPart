using PrinterServiceWebPart.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using HelixToolkit;
using HelixToolkit.Wpf;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using Assimp;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;
using System.Threading;
using System.Windows.Threading;

namespace PrinterServiceWebPart.Controllers
{
    public class ModelController : Controller
    {
        private readonly ModelRepository _modelRepo;

        public ModelController(ModelRepository modelRepo)
        {
            _modelRepo = modelRepo;
        }

        [HttpGet]
        [OutputCache(Duration = 3600, Location = OutputCacheLocation.Server)]
        public ActionResult Render(Guid modelId)
        {
            // Получаем данные модели из репозитория
            var model = _modelRepo.GetById(modelId);
            if (model == null) return HttpNotFound();

            // Генерируем превью на основе ModelData
            var previewImage = GeneratePreview(model.Filepath);

            return File(previewImage, "image/png");
        }



        private byte[] GeneratePreview(string modelPath)
        {
            //// Реализация генерации изображения
            //using (var bitmap = new Bitmap(200, 100))
            //using (var graphics = Graphics.FromImage(bitmap))
            //{
            //    graphics.Clear(System.Drawing.Color.White);
            //    graphics.DrawString(modelPath, new Font("Arial", 10), System.Drawing.Brushes.Black, new PointF(10, 10));

            //    using (var stream = new MemoryStream())
            //    {
            //        bitmap.Save(stream, ImageFormat.Png);
            //        return stream.ToArray();
            //    }
            //}

            // Загрузка модели с помощью AssimpNet
            string uploadsFolder = Server.MapPath("~/Uploads/");

            // Собираем полный путь к файлу модели
            string fullFilePath = Path.Combine(uploadsFolder, modelPath);
            var importer = new AssimpContext();
            var scene = importer.ImportFile(fullFilePath, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);
            if (scene == null || scene.MeshCount == 0)
                throw new Exception("Не удалось загрузить модель или в модели отсутствуют меши.");

            int width = 800;
            int height = 600;
            byte[] imageBytes = null;

            // Создаем скрытое окно GameWindow для offscreen-рендеринга
            // Параметры: ширина, высота, режим графики, заголовок, флаги окна, устройство отображения, версия OpenGL 3.3, флаги контекста
            using (var gameWindow = new GameWindow(width, height, GraphicsMode.Default, "Offscreen",
                                                     GameWindowFlags.Default, DisplayDevice.Default, 3, 3, GraphicsContextFlags.Offscreen))
            {
                gameWindow.MakeCurrent();

                // Настройка OpenGL: размер вьюпорта, очистка буфера цвета и глубины, включаем тест глубины
                GL.Viewport(0, 0, width, height);
                GL.ClearColor(System.Drawing.Color.CornflowerBlue);
                GL.Enable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                // Устанавливаем матрицы проекции и модели (вид камеры)
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(OpenTK.MathHelper.PiOver4, width / (float)height, 0.1f, 100f);
                Matrix4 modelview = Matrix4.LookAt(new Vector3(3, 3, 3), Vector3.Zero, Vector3.UnitY);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadMatrix(ref projection);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadMatrix(ref modelview);

                // Рендеринг первого меша модели
                var mesh = scene.Meshes[0];
                GL.Begin(PrimitiveType.Triangles);
                for (int i = 0; i < mesh.FaceCount; i++)
                {
                    var face = mesh.Faces[i];
                    foreach (var index in face.Indices)
                    {
                        // Если имеются нормали, задаем их
                        if (mesh.HasNormals)
                        {
                            var normal = mesh.Normals[index];
                            GL.Normal3(normal.X, normal.Y, normal.Z);
                        }
                        var vertex = mesh.Vertices[index];
                        GL.Vertex3(vertex.X, vertex.Y, vertex.Z);
                    }
                }
                GL.End();

                // Завершаем рендеринг и обновляем буфер
                gameWindow.SwapBuffers();
                GL.Finish();

                // Чтение пиксельных данных из буфера кадра (формат BGRA, 8 бит на канал)
                byte[] pixels = new byte[width * height * 4];
                GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

                // Создаем Bitmap из считанных пикселей
                using (var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                    int stride = data.Stride;
                    // Данные из OpenGL приходят снизу вверх, поэтому копируем строки в перевернутом порядке
                    for (int y = 0; y < height; y++)
                    {
                        IntPtr destPtr = data.Scan0 + (height - 1 - y) * stride;
                        Marshal.Copy(pixels, y * width * 4, destPtr, width * 4);
                    }
                    bitmap.UnlockBits(data);

                    // Сохраняем Bitmap в MemoryStream в формате PNG
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        imageBytes = ms.ToArray();
                    }
                }
            }

            return imageBytes;
        }

        [HttpGet]
        [OutputCache(Duration = 3600, Location = OutputCacheLocation.Server)]
        public ActionResult DownloadModel(Guid modelId)
        {
            var model = _modelRepo.GetById(modelId);
            if (model == null) return HttpNotFound();
            string uploadsFolder = Server.MapPath("~/Uploads/");

            // Собираем полный путь к файлу модели
            string fullFilePath = Path.Combine(uploadsFolder, model.Filepath);
            // Читаем .obj-файл
            var fileBytes = System.IO.File.ReadAllBytes(fullFilePath);
            return File(fileBytes, "text/plain", $"{modelId}.obj"); // MIME: text/plain или model/obj
        }
    }
}