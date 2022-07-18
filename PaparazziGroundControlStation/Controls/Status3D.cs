using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;

namespace PaparazziGroundControlStation.Controls
{

    public partial class Status3D : UserControl
    {

        Device device = null;              // Rendering device
        Mesh mesh = null;                  // Mesh object in system memory
        Material[] meshMaterials; // Materials for the mesh
        Texture[] meshTextures;            // Textures for the mesh
        public float Pitch = 0;
        public float Roll = 0;
        public float Yaw = 0;

        public Status3D()
        {
            InitializeComponent();
            InitializeDirect3D();
        }
        public bool InitializeDirect3D()
        {
            try
            {
                PresentParameters pps = new PresentParameters();
                //pps.BackBufferFormat = Format.R8G8B8;
                pps.BackBufferHeight = 128;
                pps.BackBufferWidth = 128;
                pps.BackBufferCount = 1; //backbuffer number
                pps.AutoDepthStencilFormat = DepthFormat.D16; //Z/Stencil buffer formats
                pps.EnableAutoDepthStencil = true; //active Z/Stencil buffer 
                pps.DeviceWindowHandle = this.Handle; //handle del form
                pps.SwapEffect = SwapEffect.Discard; //rendering type
                pps.Windowed = true; // Specify that it will be in a window
                device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, pps); // Put everything into the device
                device.DeviceReset += OnResetDevice;

                render_timer.Enabled = true;

                return true;
            }
            catch (DirectXException e)
            {
                MessageBox.Show(e.ToString(), "Error"); // Handle all the exceptions
                return false;
            }
        }
        public void OnResetDevice(object sender, EventArgs e)
        {
            String file3dAddress = Directory.GetCurrentDirectory() + "\\Model\\3d_model.x";

            ExtendedMaterial[] materials = null;

            Device dev = (Device)sender;
            try
            {
                // Turn on the z-buffer.
                dev.RenderState.ZBufferEnable = true;
                // Turn on ambient lighting.
                dev.RenderState.Ambient = Color.White;
                // Load the mesh from the specified file.
                if (File.Exists(file3dAddress) == false)
                {
                    return;
                }

                mesh = Mesh.FromFile(file3dAddress,
                                 MeshFlags.Managed,
                                 device,
                                 out materials);
                if (meshTextures == null)
                {
                    // Extract the material properties and texture names.
                    meshTextures = new Texture[materials.Length];
                    meshMaterials = new Material[materials.Length];
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i].Material3D != null)
                            meshMaterials[i] = materials[i].Material3D;
                        // Set the ambient color for the material. Direct3D
                        // does not do this by default.
                        if (meshMaterials[i].Diffuse != null)
                            meshMaterials[i].Ambient = meshMaterials[i].Diffuse;
                        // Create the texture.
                        if (materials[i].TextureFilename != null)
                            meshTextures[i] = TextureLoader.FromFile(dev, Directory.GetCurrentDirectory()  + "\\Model\\" + materials[i].TextureFilename);

                    }
                }
                device.RenderState.ZBufferEnable = true; //Z buffer on
                device.RenderState.Lighting = false; //lights off
                device.RenderState.ShadeMode = ShadeMode.Gouraud; //gouraud mode

                device.Transform.World = Matrix.Identity;
                device.Transform.View = Matrix.LookAtLH(new Vector3(0, 0, -20), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                device.Transform.Projection = Matrix.PerspectiveFovLH((float)(Math.PI / 3.0f), 1.0f, 1, 2000);



            }
            catch (DirectXException ea)
            {
                MessageBox.Show(ea.ToString(), "Error"); // Handle all the exceptions
            }
        }
        private void Render()
        {
            try
            {
                
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, BackColor, 1.0f, 0); // Clear the window to white
                device.BeginScene();
                //device.Transform.World = Matrix.Identity;
                if (meshMaterials == null || meshTextures == null)
                {
                    device.EndScene();
                    device.Present();
                    return;
                }
                for (int i = 0; i < meshMaterials.Length; i++)
                {
                    // Set the material and texture for this subset.
                    device.Material = meshMaterials[i];
                    device.SetTexture(0, meshTextures[i]);

                    // Draw the mesh subset.
                    Matrix m = new Matrix();
                    m = Matrix.Identity;
                    m.RotateYawPitchRoll(Yaw, Pitch,Roll);
                    device.SetTransform(TransformType.World,m);
                    mesh.DrawSubset(i);
                }
                device.RenderState.Lighting = true;
                device.Lights[0].Enabled = true;
                device.Lights[0].Type = LightType.Point;
                device.Lights[0].Diffuse = Color.White;
                device.Lights[0].Direction = new Vector3(-250, -500, 0);
                device.Lights[0].Position = new Vector3(250, 500, 0);
                device.Lights[0].Range = 5000;
                device.Lights[0].Attenuation0 = 0.1f;
                device.Lights[0].Attenuation1 = device.Lights[0].Attenuation0 / 400;
                device.Lights[0].Update();
                // End the scene.
                device.EndScene();
                device.Present();
            }
            catch (Direct3DXException d)
            {

            }
        }

        private void render_timer_Tick(object sender, EventArgs e)
        {
            Render();
        }
    }
}
