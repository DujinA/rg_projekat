// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using AssimpSample.Services;
using SharpGL.SceneGraph.Cameras;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        //parametri za interakciju preko wpf kontrola
        public int RightCarTranslation { get; set; }
        public int LeftCarRotation { get; set; }
        //public float LightSourceColor { get; set; }

        private enum TextureObjects { Asphalt = 0, Fence, Gravel }
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;

        private uint[] m_textures = null;
        private string[] m_textureFiles =
        {
            "C:\\Users\\Aleksandar\\Documents\\Code\\Github\\rg_projekat\\AssimpSample\\Images\\Asphalt.jpg",
            "C:\\Users\\Aleksandar\\Documents\\Code\\Github\\rg_projekat\\AssimpSample\\Images\\Fence.jpg",
            "C:\\Users\\Aleksandar\\Documents\\Code\\Github\\rg_projekat\\AssimpSample\\Images\\Gravel.jpg"
        };

        ///// <summary>
        /////	 Ugao rotacije Meseca
        ///// </summary>
        //private float m_moonRotation = 0.0f;

        ///// <summary>
        /////	 Ugao rotacije Zemlje
        ///// </summary>
        //private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene_williams;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene_renault;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 7000.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        private SafetyWalls _safetyWalls;

        public float LeftCarMovement { get; set; }

        public float RightCarMovement { get; set; }

        public bool CarCrossesFinnishLine { get; set; }

        private DispatcherTimer timer1;
        private DispatcherTimer timer2;
        private DispatcherTimer timer3;
        private DispatcherTimer timer4;

        private int sirina;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene SceneWilliams
        {
            get { return m_scene_williams; }
            set { m_scene_williams = value; }
        }

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene SceneRenault
        {
            get { return m_scene_renault; }
            set { m_scene_renault = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public int Sirina {
            get { return sirina; }
            set { sirina = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            //inicijalizacija oba bolida
            var scenePathForRenault = "C:\\Users\\Aleksandar\\Documents\\Code\\Github\\rg_projekat\\AssimpSample\\3D Models\\Renault";
            sceneFileName = "intento1.lwo";
            this.m_scene_renault = new AssimpScene(scenePathForRenault, sceneFileName, gl);

            var scenePathForWilliams = "C:\\Users\\Aleksandar\\Documents\\Code\\Github\\rg_projekat\\AssimpSample\\3D Models\\Williams";
            sceneFileName = "Arrows 2001.3ds";
            this.m_scene_williams = new AssimpScene(scenePathForWilliams, sceneFileName, gl);

            this.m_width = width;
            this.m_height = height;
            _safetyWalls = new SafetyWalls(this);

            m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1.0f, 1.0f, 1.0f);
            gl.Enable(OpenGL.GL_DEPTH_TEST); //ukljucivanje testiranja dubine
            gl.Enable(OpenGL.GL_CULL_FACE); //sakrivanje nevidljivih povrsina

            InitializeLightSource(gl);
            AnimationTimers();
            InitializeTextures(gl);

            m_scene_renault.LoadScene();
            m_scene_renault.Initialize();
            m_scene_williams.LoadScene();
            m_scene_williams.Initialize();
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            ResetProjection(gl);
            LightUpLeft(gl);
            LightAboveCars(gl);

            gl.PushMatrix();
            Transformations(gl);
            ScaleProjection(gl);

            InitializeCamera(gl);

            InitializeBase(gl);
            InitializeRenault(gl);
            InitializeWilliams(gl);
            InitializeTrack(gl);
            InitializeSafetyWalls(gl);

            gl.PopMatrix();

            InitializeText(gl);

            // Oznaci kraj iscrtavanja
            gl.Flush();
        }


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            ResetProjection(gl);
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene_renault.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode

        #region Pomocne metode

        private void InitializeRenault(OpenGL gl)
        {
            gl.Color(0.9, 0.9, 0.9, 1.0);
            float[] pozicija = { 0.0f, 10.0f, 0.0f, -1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, pozicija);

            gl.PushMatrix();
            float carMovement = LeftCarMovement;
            carMovement = Clamp(carMovement, 0f, 100f);
            gl.Translate(-10.0f, 40.0f - carMovement, 1.5f);
            gl.Rotate(90.0f, 90.0f, 0.0f);
            gl.Scale(1.0f, 1.0f, 1.0f);
            m_scene_renault.Draw();
            gl.PopMatrix();
        }

        private void InitializeWilliams(OpenGL gl)
        {
            gl.Color(0.9, 0.9, 0.9, 1.0);
            float[] pozicija = { 0.0f, 25.0f, 0.0f, 0.2f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, pozicija);

            gl.PushMatrix();
            float carMovement = RightCarMovement;
            carMovement = Clamp(carMovement, 0f, 100f);
            gl.Translate(10.0f, 40.0f - carMovement, 1.5f);
            gl.Rotate(90.0f, 180.0f, 0.0f);
            gl.Scale(2.5f, 2.5f, 2.5f);
            m_scene_williams.Draw();
            gl.PopMatrix();
        }

        private void InitializeBase(OpenGL gl)
        {
            gl.PushMatrix();

            var baseCoefficient = 5;
            var baseCoordinate = 10.0f;
            gl.Color(0.32f, 0.32f, 0.32f);
            gl.FrontFace(OpenGL.GL_CCW);

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            gl.Scale(3.0f, 3.0f, 3.0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Gravel]);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0.0f, 1.0f, 0.0f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-baseCoordinate * baseCoefficient, -baseCoordinate * baseCoefficient);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(baseCoordinate * baseCoefficient, -baseCoordinate * baseCoefficient);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(baseCoordinate * baseCoefficient, baseCoordinate * baseCoefficient);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(-baseCoordinate * baseCoefficient, baseCoordinate * baseCoefficient);
            gl.End();

            gl.PopMatrix();

            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.PopMatrix();
        }

        private void InitializeTrack(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(0.0f, 70.0f, 0.1f);

            var heightTrack = 120.0f;
            var widthTrack = 20.0f;

            gl.Color(0.58f, 0.58f, 0.58f);

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            gl.Scale(5.0f, 5.0f, 5.0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Asphalt]);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0.0f, 1.0f, 0.0f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-widthTrack, -widthTrack);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(-widthTrack, -heightTrack);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(widthTrack, -heightTrack);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(widthTrack, -widthTrack);
            gl.End();

            gl.PopMatrix();

            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.PopMatrix();
        }

        private void InitializeSafetyWalls(OpenGL gl)
        {
            gl.Disable(OpenGL.GL_CULL_FACE);

            gl.Color(0.63f, 0.65f, 0.66f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Fence]);

            _safetyWalls.LeftSafetyWall(gl);
            _safetyWalls.RightSafetyWall(gl);

            gl.Enable(OpenGL.GL_CULL_FACE);
        }

        private void InitializeText(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Viewport(m_width / 2, 0, m_width / 2, m_height / 2);     //donji desni ugao
            gl.DrawText3D("Arial", 14, 0, 0, "");
            gl.DrawText(sirina - 450, 150, 0.0f, 1.0f, 1.0f, "Arial", 14, "Predmet: Racunarska grafika");
            gl.DrawText(sirina - 450, 120, 0.0f, 1.0f, 1.0f, "Arial", 14, "Sk.god: 2021/2022.");
            gl.DrawText(sirina - 450, 90, 0.0f, 1.0f, 1.0f, "Arial", 14, "Ime: Aleksandar");
            gl.DrawText(sirina - 450, 60, 0.0f, 1.0f, 1.0f, "Arial", 14, "Prezime: Dujin");
            gl.DrawText(sirina - 450, 30, 0.0f, 1.0f, 1.0f, "Arial", 14, "Sifra zad: 8.1");
            gl.PopMatrix();

            gl.PopMatrix();
        }

        private void ResetProjection(OpenGL gl)
        {
            gl.Viewport(0, 0, m_width, m_height); //kreiranje viewporta po celom prozoru
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(50f, (double)m_width / m_height, 1f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        private void Transformations(OpenGL gl)
        {
            gl.Translate(0.0f, 1.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
        }

        private void InitializeLightSource(OpenGL gl)
        {
            float[] global_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);
            float[] light0specular = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };

            LightUpLeft(gl);
            LightAboveCars(gl);


            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_LIGHT1);


            // Definisemo belu spekularnu komponentu materijala sa jakim odsjajem
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR, light0specular);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SHININESS, 128.0f);

            //Uikljuci color tracking mehanizam
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);

            // Podesi na koje parametre materijala se odnose pozivi glColor funkcije
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            // Ukljuci automatsku normalizaciju nad normalama
            gl.Enable(OpenGL.GL_NORMALIZE);


            gl.ShadeModel(OpenGL.GL_SMOOTH);
        }

        private void LightAboveCars(OpenGL gl)
        {
            float[] light1pos = new float[] { 0.0f, 10.0f, m_sceneDistance, 1.0f };
            float[] light1ambient = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            float[] light1diffuse = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] light1specular = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };
            float[] direction = { 0.0f, -1.0f, 0.0f };

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, light1specular);

            // Podesi parametre reflektorkskog izvora
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, direction);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 45.0f); //TACKASTI IZVOR
        }

        private void LightUpLeft(OpenGL gl)
        {
            float[] light0pos = new float[] { -10.0f, 100.0f, m_sceneDistance, 1.0f };
            float[] light0ambient = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            float[] light0diffuse = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] light0specular = new float[] { 0.8f, 0.8f, 0.6f, 1.0f };


            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f); //TACKASTI IZVOR
        }

        private void InitializeTextures(OpenGL gl)
        {

            gl.Enable(OpenGL.GL_TEXTURE_2D);


            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);


            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {

                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);


                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);		// Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);		// Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                // Posto je kreirana tekstura slika nam vise ne treba
                image.UnlockBits(imageData);
                image.Dispose();
            }
        }

        private void InitializeCamera(OpenGL gl)
        {
            gl.LookAt(0, -10, 5, 0, 0, 0, 0, 0, 1);
        }

        private void ScaleProjection(OpenGL gl)
        {
            gl.Scale(50.0f, 50.0f, 50.0f);
        }

        public void AnimationActivate()
        {
            timer1.Start();
        }

        public void AnimationDeactivate()
        {
            timer1.Stop();
            timer2.Stop();
            timer3.Stop();
            timer4.Stop();
        }

        private void AnimationTimers()
        {
            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(20);
            timer1.Tick += new EventHandler(InitialAnimation);

            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(30);
            timer2.Tick += new EventHandler(CameraPerspectiveChange);

            timer3 = new DispatcherTimer();
            timer3.Interval = TimeSpan.FromMilliseconds(20);
            timer3.Tick += new EventHandler(UpdateAnimation3);

            timer4 = new DispatcherTimer();
            timer4.Interval = TimeSpan.FromSeconds(3f);
            timer4.Tick += new EventHandler(UpdateAnimation4);

        }

        private void InitialAnimation(object sender, EventArgs e)
        {
            SceneDistance = 1200.0f;
            RotationX = -30.0f;
            RotationY = 0.0f;


            timer1.Stop();
            timer3.Stop();
            timer4.Stop();

            timer2.Start();
        }

        private void CameraPerspectiveChange(object sender, EventArgs e)
        {
            SceneDistance -= 60.0f;
            if (SceneDistance <= -2100.0f)
            {
                timer2.Stop();

                SceneDistance = 7000.0f;
                RotationX = 120.0f;
                RotationY = 180.0f;

                timer3.Start();
                timer4.Start();
            }
        }

        private void UpdateAnimation3(object sender, EventArgs e)
        {
            if (!CarCrossesFinnishLine)
            {
                LeftCarMovement += 5.0f;
                RightCarMovement += 7.5f;
            } else
            {
                LeftCarMovement -= 1.5f;
                RightCarMovement -= 3.5f;
            }
        }

        private void UpdateAnimation4(object sender, EventArgs e)
        {
            if (!CarCrossesFinnishLine)
            {
                LeftCarMovement += 0f;
                RightCarMovement += 0f;
            }
            CarCrossesFinnishLine = !CarCrossesFinnishLine;
        }

        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        #endregion
    }
}
