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

        internal bool DrawSprite(byte[] memory, ushort spriteAddress, int startX, int startY, int height)
        {
            bool collision = false;

            //this puts 0 to display for testing
            for (int row = 0; row < height; row++)
            {
                byte sprite = memory[spriteAddress + row];
                for (int col = 0; col < SPRITE_MAX_WIDTH; col++)
                {
                    if ((sprite & (0x80 >> col)) != 0)
                    {
                        int x = (startX + col) % DISPLAY_WIDTH;
                        int y = (startY + row) % DISPLAY_HEIGHT;

                        bool oldPixel = getPixel(x, y);
                        if (oldPixel)
                        {
                            // If the pixel was already set, we have a collision
                            collision = true; // Indicate a collision
                        }

                        setPixel(x, y, oldPixel ^ true); // XOR the pixel
                    }
                }
            }

            return collision;
        }
    }
}
