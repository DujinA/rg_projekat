using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;


namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();

            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models"), "renault-f1-2004.obj", (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Sirina = (int)window.ActualWidth;
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private bool DownLifting()
        {
            if (m_world.RotationX > -20)
                return true;
            else
                return false;
        }

        private bool UpLifting()
        {
            if (m_world.RotationX < 145)
                return true;
            else
                return false;
        }

        private bool LeftSideLifting()
        {
            if (m_world.RotationY > -90)
                return true;
            else
                return false;
        }

        private bool RightSideLifting()
        {
            if (m_world.RotationY < 90)
                return true;
            else
                return false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F4: this.Close(); break;
                case Key.I:
                    if (!StopInteraction)
                    {
                        if (DownLifting())
                            m_world.RotationX -= 5.0f;
                        else
                            m_world.RotationX -= -5.0f;
                    }
                    break;
                case Key.K:
                    if (!StopInteraction)
                    {
                        if (UpLifting())
                            m_world.RotationX += 5.0f;
                        else
                            m_world.RotationX += -5.0f;
                    }
                    break;
                case Key.J:
                    if (!StopInteraction)
                    {
                        if (LeftSideLifting())
                            m_world.RotationY -= 5.0f;
                        else
                            m_world.RotationY += 5.0f;
                    }
                    break;
                case Key.L:
                    if (!StopInteraction)
                    {
                        if (RightSideLifting())
                            m_world.RotationY += 5.0f;
                        else
                            m_world.RotationY -= 5.0f;
                    }
                    break;
                case Key.Add:
                    if (!StopInteraction)
                    {
                        m_world.SceneDistance -= 700.0f;
                    }
                    break;
                case Key.Subtract:
                    if (!StopInteraction)
                    {
                        m_world.SceneDistance += 700.0f;
                    }
                    break;
                case Key.V:
                    m_world.AnimationActivate();
                    StopInteraction = true;
                    break;
                case Key.P:
                    m_world.AnimationDeactivate();
                    StopInteraction = false;
                    break;
                case Key.F2:
                    OpenFileDialog opfModel = new OpenFileDialog();
                    bool result = (bool) opfModel.ShowDialog();
                    if (result)
                    {

                        try
                        {
                            World newWorld = new World(Directory.GetParent(opfModel.FileName).ToString(), Path.GetFileName(opfModel.FileName), (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
                            m_world.Dispose();
                            m_world = newWorld;
                            m_world.Initialize(openGLControl.OpenGL);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta:\n" + exp.Message, "GRESKA", MessageBoxButton.OK );
                        }
                    }
                    break;
            }
        }
        public bool StopInteraction { get; set; }
    }
}
