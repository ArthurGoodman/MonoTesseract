using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoTesseract {
    public class Game : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        BasicEffect effect;
        VertexPositionColor[] sidesVertices;
        VertexPositionColor[] meshVertices;

        VertexBuffer sidesVertexBuffer;
        VertexBuffer meshVertexBuffer;
        IndexBuffer sidesIndexBuffer;
        IndexBuffer meshIndexBuffer;

        RasterizerState rasterizerState;

        MouseState prevMouseState, mouseState;
        KeyboardState prevKeyboardState, keyboardState;

        Vector4[] actualVertices;

        Vector2 windowedResolution;
        Vector2 fullscreenResolution;

        float scaleFactor = 2.0f;

        public Game() {
            graphics = new GraphicsDeviceManager(this);
            
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
        }

        protected override void Initialize() {
            windowedResolution = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            fullscreenResolution = new Vector2(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);

            rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            rasterizerState.MultiSampleAntiAlias = true;
            GraphicsDevice.RasterizerState = rasterizerState;

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead; 

            effect = new BasicEffect(GraphicsDevice);
            effect.VertexColorEnabled = true;

            var cameraPosition = new Vector3(0, 0, 10);
            var cameraLookAtVector = Vector3.Zero;
            var cameraUpVector = Vector3.UnitY;

            effect.View = Matrix.CreateLookAt(cameraPosition, cameraLookAtVector, cameraUpVector);
            effect.World = Matrix.Multiply(effect.World, Matrix.CreateScale(scaleFactor));

            actualVertices = new Vector4[16];

            actualVertices[0] = new Vector4(-1, -1, -1, -1);
            actualVertices[1] = new Vector4(1, -1, -1, -1);
            actualVertices[2] = new Vector4(-1, 1, -1, -1);
            actualVertices[3] = new Vector4(1, 1, -1, -1);

            actualVertices[4] = new Vector4(-1, -1, 1, -1);
            actualVertices[5] = new Vector4(1, -1, 1, -1);
            actualVertices[6] = new Vector4(-1, 1, 1, -1);
            actualVertices[7] = new Vector4(1, 1, 1, -1);

            actualVertices[8] = new Vector4(-1, -1, -1, 1);
            actualVertices[9] = new Vector4(1, -1, -1, 1);
            actualVertices[10] = new Vector4(-1, 1, -1, 1);
            actualVertices[11] = new Vector4(1, 1, -1, 1);

            actualVertices[12] = new Vector4(-1, -1, 1, 1);
            actualVertices[13] = new Vector4(1, -1, 1, 1);
            actualVertices[14] = new Vector4(-1, 1, 1, 1);
            actualVertices[15] = new Vector4(1, 1, 1, 1);

            sidesVertices = new VertexPositionColor[actualVertices.Length];
            meshVertices = new VertexPositionColor[actualVertices.Length];

            for (int i = 0; i < actualVertices.Length; i++)
                meshVertices[i].Color = new Color(0.0f, 0.0f, 0.0f, 0.2f);

            CopyVertices();

            sidesVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), sidesVertices.Length, BufferUsage.WriteOnly);
            meshVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), sidesVertices.Length, BufferUsage.WriteOnly);

            sidesVertexBuffer.SetData(sidesVertices);
            meshVertexBuffer.SetData(meshVertices);

            short[] sidesIndices = new short[] {
                0, 1, 2, 1, 2, 3,
                4, 5, 6, 5, 6, 7,
                0, 4, 2, 4, 2, 6,
                0, 1, 4, 1, 4, 5,
                2, 3, 6, 3, 6, 7,
                1, 3, 5, 3, 5, 7,

                8, 9, 10, 9, 10, 11,
                12, 13, 14, 13, 14, 15,
                8, 12, 10, 12, 10, 14,
                8, 9, 12, 9, 12, 13,
                10, 11, 14, 11, 14, 15,
                9, 11, 13, 11, 13, 15,

                0, 1, 8, 1, 8, 9,
                2, 3, 10, 3, 10, 11,
                4, 5, 12, 5, 12, 13,
                6, 7, 14, 7, 14, 15,

                0, 4, 8, 4, 8, 12,
                2, 6, 10, 6, 10, 14,
                1, 5, 9, 5, 9, 13,
                3, 7, 11, 7, 11, 15,

                0, 2, 8, 2, 8, 10,
                1, 3, 9, 3, 9, 11,
                4, 6, 12, 6, 12, 14,
                5, 7, 13, 7, 13, 15
            };

            sidesIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), sidesIndices.Length, BufferUsage.WriteOnly);
            sidesIndexBuffer.SetData(sidesIndices);

            short[] meshIndices = new short[] {
                0, 1, 1, 3, 3, 2, 2, 0,
                4, 5, 5, 7, 7, 6, 6, 4,
                0, 4, 1, 5, 2, 6, 3, 7,

                8, 9, 9, 11, 11, 10, 10, 8,
                12, 13, 13, 15, 15, 14, 14, 12,
                8, 12, 9, 13, 10, 14, 11, 15,

                0, 8, 1, 9, 2, 10, 3, 11,
                4, 12, 5, 13, 6, 14, 7, 15
            };

            meshIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), sidesIndices.Length, BufferUsage.WriteOnly);
            meshIndexBuffer.SetData(meshIndices);

            base.Initialize();
        }

        private void ToggleFullScreen() {
            graphics.IsFullScreen = !graphics.IsFullScreen;

            if (graphics.IsFullScreen)
                SetResolution(windowedResolution);
            else
                SetResolution(fullscreenResolution);
        }

        private void SetResolution(Vector2 resolution) {
            graphics.PreferredBackBufferWidth = (int)resolution.X;
            graphics.PreferredBackBufferHeight = (int)resolution.Y;

            graphics.ApplyChanges();
        }

        private void CopyVertices() {
            IEnumerable<float> zs = actualVertices.Select(v => v.Z);
            float min = zs.Min();
            float max = zs.Max();

            for (int i = 0; i < actualVertices.Length; i++) {
                meshVertices[i].Position = sidesVertices[i].Position = new Vector3(actualVertices[i].X, actualVertices[i].Y, actualVertices[i].Z);

                //float f = MathHelper.SmoothStep(0.25f, 0.9f, (actualVertices[i].Z - min) / (max - min));
                float f = MathHelper.SmoothStep(0.1f, 0.15f, (actualVertices[i].Z - min) / (max - min));
                sidesVertices[i].Color = new Color(f, f, f, 0.1f);
            }
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent() {
        }

        private bool KeyPressed(Keys key) {
            return !prevKeyboardState.IsKeyDown(key) && keyboardState.IsKeyDown(key);
        }

        protected override void Update(GameTime gameTime) {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                if (graphics.IsFullScreen)
                    ToggleFullScreen();
                else
                    Exit();

            prevMouseState = mouseState;
            mouseState = Mouse.GetState();

            prevKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();

            if (mouseState != prevMouseState) {
                int delta = mouseState.ScrollWheelValue - prevMouseState.ScrollWheelValue;
                float factor = delta > 0 ? 1.1f : delta < 0 ? 1.0f / 1.1f : 1.0f;

                scaleFactor *= factor;
                effect.World = Matrix.Multiply(effect.World, Matrix.CreateScale(factor));

                double deltaX = (mouseState.Position.X - prevMouseState.Position.X) / 50.0f / scaleFactor;
                double deltaY = -(mouseState.Position.Y - prevMouseState.Position.Y) / 50.0f / scaleFactor;

                Matrix matrix = Matrix.Identity;

                if (mouseState.LeftButton == ButtonState.Pressed) {
                    if (Math.Abs(deltaX) > 0)
                        matrix = Matrix.Multiply(matrix, new Matrix(
                            (float)Math.Cos(deltaX), 0, (float)-Math.Sin(deltaX), 0,
                            0, 1, 0, 0,
                            (float)Math.Sin(deltaX), 0, (float)Math.Cos(deltaX), 0,
                            0, 0, 0, 1));

                    if (Math.Abs(deltaY) > 0)
                        matrix = Matrix.Multiply(matrix, new Matrix(
                            1, 0, 0, 0,
                            0, (float)Math.Cos(deltaY), (float)-Math.Sin(deltaY), 0,
                            0, (float)Math.Sin(deltaY), (float)Math.Cos(deltaY), 0,
                            0, 0, 0, 1));
                } else if (mouseState.RightButton == ButtonState.Pressed) {
                    if (Math.Abs(deltaX) > 0)
                        matrix = Matrix.Multiply(matrix, new Matrix(
                            (float)Math.Cos(deltaX), 0, 0, (float)-Math.Sin(deltaX),
                            0, 1, 0, 0,
                            0, 0, 1, 0,
                            (float)Math.Sin(deltaX), 0, 0, (float)Math.Cos(deltaX)));

                    if (Math.Abs(deltaY) > 0)
                        matrix = Matrix.Multiply(matrix, new Matrix(
                            1, 0, 0, 0,
                            0, (float)Math.Cos(deltaY), 0, (float)-Math.Sin(deltaY),
                            0, 0, 1, 0,
                            0, (float)Math.Sin(deltaY), 0, (float)Math.Cos(deltaY)));
                }

                for (int i = 0; i < actualVertices.Length; i++)
                    actualVertices[i] = Vector4.Transform(actualVertices[i], matrix);

                CopyVertices();

                sidesVertexBuffer.SetData(sidesVertices);
                meshVertexBuffer.SetData(meshVertices);
            }

            if (KeyPressed(Keys.F11)) {
                ToggleFullScreen();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            float aspectRatio = graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            float fieldOfView = MathHelper.PiOver4;
            float nearClipPlane = 1;
            float farClipPlane = 200;

            effect.Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClipPlane, farClipPlane);

            GraphicsDevice.SetVertexBuffer(sidesVertexBuffer);
            GraphicsDevice.Indices = sidesIndexBuffer;

            //GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (var pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, sidesVertexBuffer.VertexCount, 0, sidesIndexBuffer.IndexCount / 3);
            }

            GraphicsDevice.SetVertexBuffer(meshVertexBuffer);
            GraphicsDevice.Indices = meshIndexBuffer;

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (var pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, meshVertexBuffer.VertexCount, 0, meshIndexBuffer.IndexCount / 2);
            }

            base.Draw(gameTime);
        }
    }
}
