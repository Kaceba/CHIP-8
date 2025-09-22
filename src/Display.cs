using static CHIP_8.Constants;

namespace CHIP_8

{
    internal class Display
    {
        internal bool[] display = new bool[64 * 32];

        internal bool getPixel(int x, int y)
        {
            return display[y * 64 + x];
        }

        internal bool setPixel(int x, int y, bool value)
        {
            return display[y * 64 + x] = value;
        }

        internal void DrawCharacter(byte[] memory, byte character, int startX, int startY)
        {
            //this puts 0 to display for testing
            for (int row = 0; row < FONT_HEIGHT; row++)
            {
                byte CharacterByte = memory[character + row];
                for (int col = 0; col < FONT_WIDTH; col++)
                {
                    if ((CharacterByte & (0x80 >> col)) != 0)
                    {
                        int x = startX + col;
                        int y = startY + row;
                        if (x < DISPLAY_WIDTH && y < DISPLAY_HEIGHT)
                        {
                            setPixel(x, y, true);
                        }
                    }
                }
            }
        }
    }
}
