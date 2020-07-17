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
        static Material textMaterial;

        public static GUIStyle LabelStyle { get; }
        public static GUIStyle ButtonStyle { get; }
        public static Vector2 MousePosition { get; private set; }

        private static string lastId = "";
        private static string lastClickedId = "";
        private static int elementCount;
        private static int textCarrot;

        private static Vector2 defGuiResolution = new Vector2(1280, 720);

        static GUI()
        {
            material = AssetDatabase.GetAsset<Material>("Resources/Materials/GUI/GUI.mat");
            textMaterial = AssetDatabase.GetAsset<Material>("Resources/Materials/GUI/Text.mat");

            LabelStyle = new GUIStyle() { FontSize = 48, Font = AssetDatabase.GetAsset<Font>("Resources/Fonts/SHERWOOD.ttf"),
                HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };

            ButtonStyle = new GUIStyle() { FontSize = 48, Font = AssetDatabase.GetAsset<Font>("Resources/Fonts/SHERWOOD.ttf"),
                HorizontalAlignment = HorizontalAlignment.Middle, VerticalAlignment = VerticalAlignment.Middle};
            
            ButtonStyle.Normal = new GUIStyleOption()
            {
                Background = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/btn_normal.png"),
                TextColor = Color4.White
            }; 
            ButtonStyle.Hover = new GUIStyleOption()
            {
                Background = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/btn_hover.png"),
                TextColor = Color4.Black
            };
            ButtonStyle.Active = new GUIStyleOption()
            {
                Background = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/btn_active.png"),
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
                    GenerateImage(style.Active.Background, size, false, true);
                    GenerateTextAndRender(text, size, style, false, true);
                    lastClickedId = id;
                }
                else
                {
                    GenerateImage(style.Hover.Background, size, true, false);
                    GenerateTextAndRender(text, size, style, true, false);
                }
            }
            else
            {
                GenerateImage(style.Normal.Background, size, false, false);
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
            GenerateImage(image, size, false, false);
            elementCount++;
        }
        public static void Textbox(ref string text, float maxLength, Rect size)
        {
            Textbox(ref text, maxLength, size, ButtonStyle);
        }

        public static void Textbox(ref string text, float maxLength, Rect size, GUIStyle style)
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

                theText = text.Insert(textCarrot, "^");
            }
            if (size.IsPointInside(MousePosition))
            {
                if (Mouse.GetState().IsButtonDown(MouseButton.Left))
                {
                    GenerateImage(style.Active.Background, size, false, true);
                    GenerateTextAndRender(theText, size, style, false, true);
                    lastClickedId = id;
                }
                else
                {
                    GenerateImage(style.Hover.Background, size, true, false);
                    GenerateTextAndRender(theText, size, style, true, false);
                }
            }
            else if (lastClickedId == id)
            {
                GenerateImage(style.Active.Background, size, false, true);
                GenerateTextAndRender(theText, size, style, false, true);
            }
            else
            {
                GenerateImage(style.Normal.Background, size, false, false);
                GenerateTextAndRender(theText, size, style, false, false);
            }

            elementCount++;
        }

        static void GenerateImage(Texture image, Rect size, bool isHovered, bool isActive)
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
            material.SetTexture(0, image);
            Renderer.DrawNow(mesh, material);
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

            float maxTextHeight = 0;

            for (int i = 0; i < text.Length; i++)
            {
                var character = style.Font.RequestCharacter((char)text[i]);
                if (character.BitmapHeight > maxTextHeight)
                    maxTextHeight = character.BitmapHeight;

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
                    float yPos = y - lineOffset - (h - top) - (scaleY * maxTextHeight) - (GetYAlignment() / winHeight);
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
                        return 0;
                    case HorizontalAlignment.Middle:
                        return ((rectWidth * 2f) - (width * fontScale))/2f;
                    case HorizontalAlignment.Right:
                        return ((rectWidth * 2f) - (width * fontScale));
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
                        return 0;
                    case VerticalAlignment.Middle:
                        return ((rectHeight - height) + style.FontSize)/2f;
                    case VerticalAlignment.Bottom:
                        return (rectHeight - height) + style.FontSize;
                }

                return 0;
            }

            GL.Disable(EnableCap.DepthTest);
            Mesh mesh = new Mesh(new VertexContainer(vertices.ToArray(), uvs.ToArray()), indices.ToArray());
            textMaterial.SetTexture(0, style.Font.AtlasTexture);
            if (isActive)
                textMaterial.Shader.SetUniform("u_Color", style.Active.TextColor.ToVector4());
            else if (isHovered)
                textMaterial.Shader.SetUniform("u_Color", style.Hover.TextColor.ToVector4());
            else
                textMaterial.Shader.SetUniform("u_Color", style.Normal.TextColor.ToVector4());
            Renderer.DrawNow(mesh, textMaterial);
            mesh.Dispose();
            GL.Enable(EnableCap.DepthTest);
            {
                /*
    
                float scale = style.FontSize / style.Font.FontSize;
    
                float sx = (2f/(float) Program.Window.Width)* scale;
                float sy = (2f/(float) Program.Window.Height)* scale;
    
                float x = ((size.X / (float)Program.Window.Width) * 2) - 1;
                float startingX = x;
                float rectWidth = startingX + ((size.Width / (float)Program.Window.Width) * 2);
    
                float y = ((size.Y / (float)Program.Window.Height) * 2) - 1;
                y = -y;
                float startingY = y;
                float rectHeight = (startingY - ((size.Height * 2) / Program.Window.Height));
    
                //Calculate lines
                int curLine = 0;
                List<string> lines = new List<string>();
                List<float> linesWidth = new List<float>();
                linesWidth.Add(0);
                lines.Add("");
    
                float maxHeight = 0;
    
                for (int i = 0; i < text.Length; i++)
                {
                    var character = style.Font.RequestCharacter((char)text[i]);
    
                    if (character.BitmapHeight > maxHeight)
                        maxHeight = character.BitmapHeight * sx;
    
                    float w = character.BitmapWidth * sx;
                    float h = character.BitmapHeight * sy;
    
                    float linesCount = (lines.Count * 2) - 1;
    
                    if (linesCount <= rectHeight + h)
                        break;
    
                    //new line
                    float lineStart = (linesWidth[curLine] * 2) - 1;
                    if ((char)text[i] == '\n' || lineStart + character.AdvanceX * sx >= rectWidth - w)
                    {
                        if (text[i] != '\n')
                        {
                            lines.Add(text[i].ToString());
                            linesWidth.Add(character.AdvanceX * sx);
                        }
                        else
                        {
                            lines.Add("");
                            linesWidth.Add(0);
                        }
                        curLine++;
                    }
                    else
                    {
                        linesWidth[curLine] += character.AdvanceX * sx;
                        lines[curLine] += (char) text[i];
                    }
                }
    
                float yOff = GetYOffset();
                for (int i = 0; i < lines.Count; i++)
                {
                    float xOff = GetXOffset(i);
                    for (int j = 0; j < lines[i].Length; j++)
                    {
                        var character = style.Font.RequestCharacter((char)lines[i][j]);
                        float w = character.BitmapWidth * sx;
                        float h = character.BitmapHeight * sy;
    
                        float hDiff = (character.BitmapHeight - character.BitmapTop) * sy;
    
                        float x2 = x + xOff;
                        float y2 = y + yOff;
                        float x3 = x2 + w;
                        float y3 = y2 + h;
    
                        y2 = y2 - (maxHeight * 2) - hDiff;
                        y3 = y3 - (maxHeight * 2) - hDiff;
    
                        x += character.AdvanceX * sx;
    
                        if (w == 0 || h == 0)
                            continue;
    
                        vertices.Add(new Vector3(x2, y2, 0));
                        vertices.Add(new Vector3(x3, y2, 0));
                        vertices.Add(new Vector3(x2, y3, 0));
                        vertices.Add(new Vector3(x3, y3, 0));
    
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
                    }
                    y -= style.Font.LineHeight * sy;
                    x = startingX;
                }
    
                float GetXOffset(int line)
                {
                    float lineWidth;
                    float difference;
                    switch (style.HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left:
                            return 0;// (defGuiResolution.X / Program.Window.Width) - 1;
                        case HorizontalAlignment.Middle:
                            lineWidth = linesWidth[line];
                            difference = (rectWidth - startingX) - lineWidth;
    
                            return difference / 2;
                        case HorizontalAlignment.Right:
                            lineWidth = linesWidth[line];
                            difference = (rectWidth - startingX) - lineWidth;
    
                            return difference;
                    }
    
                    return 0;
                }
    
                float GetYOffset()
                {
                    float height = style.FontSize * lines.Count;
                    float difference;
                    float resOffset = (defGuiResolution.Y / Program.Window.Height) - 1;
    
                    switch (style.VerticalAlignment)
                    {
                        case VerticalAlignment.Top:
                            return 0;
    
                        case VerticalAlignment.Middle:
                            return ((rectHeight - startingY) + height * (sy/scale))/2f;//((size.Height - height) * sy)/2;
                        case VerticalAlignment.Bottom:
                            //height = lines.Count * style.Font.FontSize * sy;
                            //difference = -(rectHeight - startingY) - height;
    
                            return -(rectHeight - height) *(sy / scale);
                    }
    
                    return 0;
                }
    
                
                */
            }


        }
    }
}
