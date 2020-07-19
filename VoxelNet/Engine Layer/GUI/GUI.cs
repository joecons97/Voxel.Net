using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;
using VoxelNet.Input;

namespace VoxelNet
{
    public static class GUI
    {
        static Material material;
        static Material materialSliced;
        static Material textMaterial;

        public static GUIStyle LabelStyle { get; }
        public static GUIStyle ButtonStyle { get; }
        public static GUIStyle TextBoxStyle { get; }
        public static Vector2 MousePosition { get; private set; }

        private static string lastId = "";
        private static string lastClickedId = "";
        private static int elementCount;
        private static int textCarrot;

        private static Vector2 defGuiResolution = new Vector2(1280, 720);

        static GUI()
        {
            material = AssetDatabase.GetAsset<Material>("Resources/Materials/GUI/GUI.mat");
            materialSliced = AssetDatabase.GetAsset<Material>("Resources/Materials/GUI/GUI_Sliced.mat");
            textMaterial = AssetDatabase.GetAsset<Material>("Resources/Materials/GUI/Text.mat");

            LabelStyle = new GUIStyle()
            {
                FontSize = 48, Font = AssetDatabase.GetAsset<Font>("Resources/Fonts/SHERWOOD.ttf"),
                HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top
            };

            ButtonStyle = new GUIStyle()
            {
                FontSize = 48, Font = AssetDatabase.GetAsset<Font>("Resources/Fonts/SHERWOOD.ttf"),
                HorizontalAlignment = HorizontalAlignment.Middle, VerticalAlignment = VerticalAlignment.Middle,
                SlicedBorderSize = 1f
            };

            ButtonStyle.Normal = new GUIStyleOption()
            {
                Background = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/btn_normal.png"),
                TextColor = Color4.White
            };
            ButtonStyle.Hover = new GUIStyleOption()
            {
                Background = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/btn_hover.png"),
                TextColor = Color4.White
            };
            ButtonStyle.Active = new GUIStyleOption()
            {
                Background = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/btn_hover.png"),
                TextColor = Color4.White
            };

            TextBoxStyle = new GUIStyle()
            {
                FontSize = 48,
                Font = AssetDatabase.GetAsset<Font>("Resources/Fonts/SHERWOOD.ttf"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Middle, 
                AlignmentOffset = new Vector2(12, 0),
                SlicedBorderSize = 1f
            };

            TextBoxStyle.Normal = new GUIStyleOption()
            {
                Background = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/txt_normal.png"),
                TextColor = Color4.White
            };
            TextBoxStyle.Hover = new GUIStyleOption()
            {
                Background = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/txt_hover.png"),
                TextColor = Color4.White
            };
            TextBoxStyle.Active = new GUIStyleOption()
            {
                Background = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/txt_hover.png"),
                TextColor = Color4.White
            };


            Program.Window.MouseMove += (sender, args) =>
            {
                MousePosition = new Vector2(args.Position.X, args.Position.Y);
            };

            Program.Window.MouseDown += (sender, args) => { lastClickedId = ""; };

            Program.Window.KeyDown += (sender, args) => {if(args.Key == Key.Enter){ lastClickedId = "";} };
        }

        static void ClearID()
        {
            lastId = "";
        }

        public static void NewFrame()
        {
            elementCount = 0;
        }

        public static void PushID(string id)
        {
            if (string.IsNullOrEmpty(lastId))
            {
                lastId = id;
            }
        }

        public static bool Button(string text, Rect size)
        {
            return Button(text, size, ButtonStyle);
        }

        public static bool Button(string text, Rect size, GUIStyle style)
        {
            string id = (elementCount + 1).ToString();
            PushID(id);
            bool up = Mouse.GetState().IsButtonUp(MouseButton.Left);

            if (size.IsPointInside(MousePosition))
            {
                if (Mouse.GetState().IsButtonDown(MouseButton.Left))
                {
                    GenerateImage(style.Active.Background, size, false, true, style.SlicedBorderSize);
                    GenerateTextAndRender(text, size, style, false, true);
                    lastClickedId = id;
                }
                else
                {
                    GenerateImage(style.Hover.Background, size, true, false, style.SlicedBorderSize);
                    GenerateTextAndRender(text, size, style, true, false);
                }
            }
            else
            {
                GenerateImage(style.Normal.Background, size, false, false, style.SlicedBorderSize);
                GenerateTextAndRender(text, size, style, false, false);
            }

            bool ret = id == lastClickedId && up;

            if (ret)
            {
                lastClickedId = "";
                ClearID();
            }

            elementCount++;
            return ret;
        }

        public static void Label(string text, Rect size)
        {
            Label(text, size, LabelStyle);
        }

        public static void Label(string text, Rect size, GUIStyle style)
        {
            GenerateTextAndRender(text, size, style, false, false);
            elementCount++;
        }

        public static void Image(Texture image, Rect size)
        {
            GenerateImage(image, size, false, false, 0);
            elementCount++;
        }

        public static void Image(Texture image, Rect size, float sliceSize)
        {
            GenerateImage(image, size, false, false, sliceSize);
            elementCount++;
        }

        public static void Textbox(ref string text, float maxLength, Rect size)
        {
            Textbox(ref text, maxLength, size, TextBoxStyle);
        }

        public static void Textbox(ref string text,string placeHolder, float maxLength, Rect size)
        {
            Textbox(ref text, maxLength, size, TextBoxStyle, placeHolder);
        }

        public static void Textbox(ref string text, float maxLength, Rect size, GUIStyle style, string placeholder = "")
        {
            string id = (elementCount + 1).ToString();
            PushID(id);
            string theText = text;
            if (lastClickedId == id)
            {
                var args = Input.Input.GetLastKeyDown();
                if (args != null)
                {
                    var val = args.Key;
                    bool isUtility = false;
                    switch (val)
                    {
                        case Key.Unknown:
                            isUtility = true;
                            break;
                        case Key.BackSpace:
                            if (text.Length > textCarrot - 1 && textCarrot > 0)
                            {
                                text = text.Remove(textCarrot - 1, 1);
                                textCarrot--;
                            }

                            isUtility = true;
                            break;
                        case Key.Delete:
                            if (textCarrot < text.Length)
                            {
                                text = text.Remove(textCarrot, 1);
                            }

                            isUtility = true;
                            break;
                        case Key.Space:
                            text = text.Insert(textCarrot, " ");
                            textCarrot++;
                            isUtility = true;
                            break;
                        case Key.Left:
                            if (textCarrot > 0)
                                textCarrot--;

                            isUtility = true;
                            break;
                        case Key.Right:
                            if (textCarrot < text.Length)
                                textCarrot++;

                            isUtility = true;
                            break;

                        case Key.ShiftLeft:
                            isUtility = true;
                            break;
                        case Key.ShiftRight:
                            isUtility = true;
                            break;
                        case Key.CapsLock:
                            isUtility = true;
                            break;
                    }

                    if (!isUtility && theText.Length < maxLength)
                    {
                        var chara = Input.Input.GetLastKeyPress().ToString();
                        if (args.Modifiers == KeyModifiers.Shift)
                            chara = chara.ToUpper();
                        else
                            chara = chara.ToLower();
                        text = text.Insert(textCarrot, chara);
                        textCarrot++;
                    }
                }

                theText = text.Insert(textCarrot, "_");
            }

            Color4 startingColour = style.Normal.TextColor;
            if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(placeholder) && lastClickedId != id)
            {
                style.Normal.TextColor = Color4.Gray;
                style.Hover.TextColor = Color4.Gray;
                style.Active.TextColor = Color4.Gray;
                theText = placeholder;
            }

            if (size.IsPointInside(MousePosition))
            {
                if (Mouse.GetState().IsButtonDown(MouseButton.Left))
                {
                    GenerateImage(style.Active.Background, size, false, true, style.SlicedBorderSize);
                    GenerateTextAndRender(theText, size, style, false, true);
                    lastClickedId = id;
                }
                else
                {
                    GenerateImage(style.Hover.Background, size, true, false, style.SlicedBorderSize);
                    GenerateTextAndRender(theText, size, style, true, false);
                }
            }
            else if (lastClickedId == id)
            {
                GenerateImage(style.Active.Background, size, false, true, style.SlicedBorderSize);
                GenerateTextAndRender(theText, size, style, false, true);
            }
            else
            {
                GenerateImage(style.Normal.Background, size, false, false, style.SlicedBorderSize);
                GenerateTextAndRender(theText, size, style, false, false);
            }

            style.Normal.TextColor = startingColour;
            style.Hover.TextColor = startingColour;
            style.Active.TextColor = startingColour;
            elementCount++;
        }

        static void GenerateImage(Texture image, Rect size, bool isHovered, bool isActive, float sliceSize)
        {
            float x = ((size.X / Program.Window.Width) * 2) - 1;
            float y = (((Program.Window.Height - size.Y) / Program.Window.Height)*2)-1;
            float maxX = x + ((size.Width * 2) / Program.Window.Width);
            float maxY = y - ((size.Height* 2) / Program.Window.Height);

            List<uint> indices = new List<uint>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            vertices.Add(new Vector3(x, y,0));
            vertices.Add(new Vector3(x, maxY, 0));
            vertices.Add(new Vector3(maxX, y, 0));
            vertices.Add(new Vector3(maxX, maxY, 0));

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));

            indices.Add(0);
            indices.Add(1);
            indices.Add(2);

            indices.Add(1);
            indices.Add(3);
            indices.Add(2);

            GL.Disable(EnableCap.DepthTest);
            Mesh mesh = new Mesh(new VertexContainer(vertices.ToArray(), uvs.ToArray()), indices.ToArray());
            if (sliceSize > 0)
            {
                materialSliced.SetTexture(0, image);
                materialSliced.SetUniform("u_BorderSize", sliceSize);
                materialSliced.SetUniform("u_Dimensions", new Vector2(size.Width, size.Height));
                Renderer.DrawNow(mesh, materialSliced);
            }
            else
            {
                material.SetTexture(0, image);
                Renderer.DrawNow(mesh, material);
            }

            mesh.Dispose();
            GL.Enable(EnableCap.DepthTest);
        }

        static void GenerateTextAndRender(string text, Rect size, GUIStyle style, bool isHovered, bool isActive)
        {
            float winWidth = Program.Window.Width;
            float winHeight = Program.Window.Height;

            float x = ((size.X / winWidth) * 2) - 1;
            float y = (((winHeight - size.Y) / winHeight) * 2) - 1;

            uint indexCount = 0;
            List<uint> indices = new List<uint>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            List<string> lines = new List<string>();
            List<float> linesWidth = new List<float>();
            int curLine = 0;
            lines.Add("");
            linesWidth.Add(0);

            float fontScale = style.FontSize/style.Font.FontSize;
            float scaleX = ((fontScale / winWidth));// * 2) - 1;

            float scaleY = ((fontScale / winHeight));// * 2) - 1;

            for (int i = 0; i < text.Length; i++)
            {
                var character = style.Font.RequestCharacter((char)text[i]);

                if (text[curLine] == '\n')
                {
                    lines.Add("");
                    linesWidth.Add(0);
                    curLine++;
                }
                else
                {
                    if (linesWidth[curLine] + character.AdvanceX > (size.Width * 2f))
                    {
                        lines.Add(text[i].ToString());
                        linesWidth.Add(character.AdvanceX);
                        curLine++;
                    }
                    else
                    {
                        lines[curLine] += (text[i].ToString());
                        linesWidth[curLine] += (character.AdvanceX);
                    }
                }
            }

            int index = 0;
            float lineOffset = 0;
            float charOffset = x;
            foreach (var line in lines)
            {
                for (int c = 0; c < line.Length; c++)
                {
                    var character = style.Font.RequestCharacter((char)line[c]);

                    float w = ((character.BitmapWidth) * scaleX);
                    float h = ((character.BitmapHeight) * scaleY);
                    float top = ((character.BitmapTop) * scaleY);

                    float xPos = charOffset + (GetXAlignment(index)/winWidth);
                    float yPos = y - lineOffset - (h - top) - (scaleY * style.Font.FontSize) - (GetYAlignment() / winHeight);
                    float xPosEnd = xPos + w;
                    float yPosEnd = yPos + h;

                    vertices.Add(new Vector3(xPos, yPos, 0));
                    vertices.Add(new Vector3(xPosEnd, yPos, 0));
                    vertices.Add(new Vector3(xPos, yPosEnd, 0));
                    vertices.Add(new Vector3(xPosEnd, yPosEnd, 0));

                    uvs.Add(new Vector2(character.TexCoordX, character.TexCoordY + character.BitmapHeight / style.Font.GetAtlasHeight()));
                    uvs.Add(new Vector2(character.TexCoordX + character.BitmapWidth / style.Font.GetAtlasWidth(), character.TexCoordY + character.BitmapHeight / style.Font.GetAtlasHeight()));
                    uvs.Add(new Vector2(character.TexCoordX, character.TexCoordY));
                    uvs.Add(new Vector2(character.TexCoordX + character.BitmapWidth / style.Font.GetAtlasWidth(), character.TexCoordY));

                    indices.Add(indexCount + 0);
                    indices.Add(indexCount + 1);
                    indices.Add(indexCount + 2);

                    indices.Add(indexCount + 3);
                    indices.Add(indexCount + 2);
                    indices.Add(indexCount + 1);

                    indexCount += 4;

                    charOffset += character.AdvanceX * scaleX;
                }

                charOffset = x;
                index++;
                lineOffset += style.FontSize * scaleY;
            }

            float GetXAlignment(int line)
            {
                float width = linesWidth[line];
                float rectWidth = size.Width;

                switch (style.HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        return style.AlignmentOffset.X;
                    case HorizontalAlignment.Middle:
                        return style.AlignmentOffset.X + ((rectWidth * 2f) - (width * fontScale))/2f;
                    case HorizontalAlignment.Right:
                        return style.AlignmentOffset.X + ((rectWidth * 2f) - (width * fontScale));
                }

                return 0;
            }

            float GetYAlignment()
            {
                float height = style.FontSize * lines.Count;
                float rectHeight = size.Height;

                switch (style.VerticalAlignment)
                {
                    case VerticalAlignment.Top:
                        return style.AlignmentOffset.Y;
                    case VerticalAlignment.Middle:
                        return style.AlignmentOffset.Y + ((rectHeight - height) + style.Font.FontSize/2f)/2f;
                    case VerticalAlignment.Bottom:
                        return style.AlignmentOffset.Y + (rectHeight - height) + (style.Font.FontSize/2f)/2f;
                }

                return 0;
            }

            GL.Disable(EnableCap.DepthTest);
            Mesh mesh = new Mesh(new VertexContainer(vertices.ToArray(), uvs.ToArray()), indices.ToArray());
            textMaterial.SetTexture(0, style.Font.AtlasTexture);
            if (isActive)
                textMaterial.SetUniform("u_Color", style.Active.TextColor.ToVector4());
            else if (isHovered)
                textMaterial.SetUniform("u_Color", style.Hover.TextColor.ToVector4());
            else
                textMaterial.SetUniform("u_Color", style.Normal.TextColor.ToVector4());
            Renderer.DrawNow(mesh, textMaterial);
            mesh.Dispose();
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
