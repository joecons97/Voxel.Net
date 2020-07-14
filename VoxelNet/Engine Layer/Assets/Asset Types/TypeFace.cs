using System;
using System.IO;
using Ionic.Zip;
using VoxelNet.Assets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoxelNet.Rendering;

namespace VoxelNet
{ 
    public class TypeFace : IImportable
    {
		private const int PAD_TOP = 0;
		private const int PAD_LEFT = 1;
		private const int PAD_BOTTOM = 2;
		private const int PAD_RIGHT = 3;

		private const int DESIRED_PADDING = 3;

		private const char SPLITTER = ' ';
		private const char NUMBER_SEPARATOR = ',';

		public const float LINE_HEIGHT = 0.03f;
		public const int SPACE_ASCII = 32;

		private float aspectRatio;
        private float startingHeight;

		private float verticalPerPixelSize;
		private float horizontalPerPixelSize;
		private float spaceWidth;
		private int[] padding;
		private int paddingWidth;
		private int paddingHeight;
        private float baseSize;
        private int imageWidth;

        private Texture atlas;

		private Dictionary<int, Character> metaData = new Dictionary<int, Character>();

		private Dictionary<String, String> values = new Dictionary<String, String>();
		Queue<string> linesToProcess;// = new Stack<string>();

        public TypeFace() { }

        public TypeFace(string[] fontFile)
        {
            aspectRatio = (float)Program.Window.Width / (float)Program.Window.Height;
            startingHeight = (float)Program.Window.Height;
			linesToProcess = new Queue<string>(fontFile);
            LoadPaddingData();
            LoadLineSizes();
            imageWidth = GetValueOfVariable("scaleW");
            LoadTextureFile();
            LoadCharacterData(imageWidth);

            Program.Window.Resize += (sender, args) =>
            {
				aspectRatio = (float)Program.Window.Width / (float)Program.Window.Height;
				horizontalPerPixelSize = verticalPerPixelSize / aspectRatio;
                UpdateSizes();

            };
        }

		void UpdateSizes()
		{
            for (int i = 0; i < metaData.Keys.Count; i++)
            {
                metaData[metaData.Keys.ToArray()[i]].UpdateSizes(horizontalPerPixelSize, verticalPerPixelSize);
            }
		}

		public IImportable Import(string path, ZipFile pack)
        {
            string[] lines = null;
            if (pack.ContainsEntry(path))
			{
				MemoryStream stream = new MemoryStream();
                pack[path].Extract(stream);
                var text = Encoding.ASCII.GetString(stream.ToArray());
                lines = text.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].Trim('\r');
                }
			}
            else
            {
                lines = File.ReadAllLines(path);
            }

			return new TypeFace(lines);
        }

        public Texture GetAtlas()
        {
            return atlas;
        }

        public float GetSpaceWidth()
        {
            return spaceWidth * horizontalPerPixelSize;
        }

        public float GetRelativeSize(float size)
        {
            return (size / baseSize) * 5;
        }

        public Character GetCharacter(int ascii)
        {
            if (metaData.ContainsKey(ascii))
                return metaData[ascii];

            return null;
        }

        void LoadTextureFile()
        {
            ProcessNextLine();
            string loc = GetValueOfVariableStr("file").Trim('"');
            atlas = AssetDatabase.GetAsset<Texture>("Resources/Fonts/" + loc);
        }

		void LoadCharacterData(int imgWidth)
		{
			ProcessNextLine();
			while(ProcessNextLine())
			{
				Character c = LoadCharacter(imgWidth);
				if(c != null)
					metaData.Add(c.GetId(), c);
			}
		}

		Character LoadCharacter(int imgSize)
		{
			int id = GetValueOfVariable("id");
			if (id == SPACE_ASCII) {
				spaceWidth = (GetValueOfVariable("xadvance") - paddingWidth);
				return null;
			}
			float xTex = ((float) GetValueOfVariable("x") + (padding[PAD_LEFT] - DESIRED_PADDING)) / imgSize;
			float yTex = ((float) GetValueOfVariable("y") + (padding[PAD_TOP] - DESIRED_PADDING)) / imgSize;
			int width = GetValueOfVariable("width") - (paddingWidth - (2 * DESIRED_PADDING));
			int height = GetValueOfVariable("height") - ((paddingHeight) - (2 * DESIRED_PADDING));
            float quadWidth = width;// * horizontalPerPixelSize;
            float quadHeight = height;// * verticalPerPixelSize;
			float xTexSize = (float) width / imgSize;
			float yTexSize = (float) height / imgSize;
            float xOff = (GetValueOfVariable("xoffset") + padding[PAD_LEFT] - DESIRED_PADDING);// * horizontalPerPixelSize;
            float yOff = (GetValueOfVariable("yoffset") + (padding[PAD_TOP] - DESIRED_PADDING));// * verticalPerPixelSize;
            float xAdvance = (GetValueOfVariable("xadvance") - paddingWidth);// * horizontalPerPixelSize;
			return new Character(id, xTex, yTex, xTexSize, yTexSize, xOff, yOff, quadWidth, quadHeight, xAdvance, horizontalPerPixelSize, verticalPerPixelSize);
		}

		void LoadPaddingData()
		{
			ProcessNextLine();
			padding = GetValuesOfVariable("padding");
			paddingWidth = padding[PAD_LEFT] + padding[PAD_RIGHT];
			paddingHeight = padding[PAD_TOP] + padding[PAD_BOTTOM];
            baseSize = GetValueOfVariable("size");
        }

		void LoadLineSizes()
		{
			ProcessNextLine();
			int lineHeight = GetValueOfVariable("lineHeight") - paddingHeight;
			verticalPerPixelSize = LINE_HEIGHT / (float)lineHeight;
			horizontalPerPixelSize = verticalPerPixelSize/aspectRatio;
		}

		int[] GetValuesOfVariable(string variable)
		{
			string[] numbers = values[variable].Split(NUMBER_SEPARATOR);
			int[] actualValues = new int[numbers.Length];
			for (int i = 0; i < actualValues.Length; i++)
			{
				actualValues[i] = int.Parse(numbers[i]);
			}
			return actualValues;
		}

		int GetValueOfVariable(string variable)
		{
			return int.Parse(values[variable]);
		}
        string GetValueOfVariableStr(string variable)
        {
            return values[variable];
        }

		bool ProcessNextLine()
		{
			values.Clear();
			string line = "";
			if(linesToProcess.Count > 0)
				line = linesToProcess.Dequeue();
			else
				return false;

			foreach (var item in line.Split(SPLITTER))
			{
				string[] pair = item.Split('=');
				if(pair.Length == 2)
				{
					values.Add(pair[0],pair[1]);
				}
			}

			return true;
		}

        public void Dispose()
        {

        }

        public class Character
        {
			private int id;
			private float xTextureCoord;
			private float yTextureCoord;
			private float xMaxTextureCoord;
			private float yMaxTextureCoord;
			private float xOffset;
			private float yOffset;
			private float sizeX;
			private float sizeY;
			private float xAdvance;
            private float horizontalPixelSize;
            private float verticalPixelSize;

			public Character(int id, float xTextureCoord, float yTextureCoord, float xTexSize, float yTexSize,
					float xOffset, float yOffset, float sizeX, float sizeY, float xAdvance, float horizontalSize, float verticalSize)
			{
				this.id = id;
				this.xTextureCoord = xTextureCoord;
				this.yTextureCoord = yTextureCoord;
				this.xOffset = xOffset;
				this.yOffset = yOffset;
				this.sizeX = sizeX;
				this.sizeY = sizeY;
				this.xMaxTextureCoord = xTexSize + xTextureCoord;
				this.yMaxTextureCoord = yTexSize + yTextureCoord;
				this.xAdvance = xAdvance;

                horizontalPixelSize = horizontalSize;
                verticalPixelSize = verticalSize;
            }

            public void UpdateSizes(float horizontalSize, float verticalSize)
			{
				horizontalPixelSize = horizontalSize;
                verticalPixelSize = verticalSize;
			}

			public int GetId()
			{
				return id;
			}

			public float GetxTextureCoord()
			{
				return xTextureCoord;
			}

			public float GetyTextureCoord()
			{
				return yTextureCoord;
			}

			public float GetXMaxTextureCoord()
			{
				return xMaxTextureCoord;
			}

			public float GetYMaxTextureCoord()
			{
				return yMaxTextureCoord;
			}

			public float GetxOffset()
			{
				return xOffset * horizontalPixelSize;
			}

			public float GetyOffset()
			{
				return yOffset * verticalPixelSize;
			}

			public float GetSizeX()
			{
				return sizeX * horizontalPixelSize;
			}

			public float GetSizeY()
			{
				return sizeY * verticalPixelSize;
			}

			public float GetxAdvance()
			{
				return xAdvance * horizontalPixelSize;
			}
		}
    }
}
