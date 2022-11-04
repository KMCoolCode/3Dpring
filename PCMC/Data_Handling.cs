using System;

namespace sun
{
    class Data_Handling
    {
        public byte[] bitTObyte(byte[] output, int num)
        {
            byte[] outputB = new byte[num];
            int i, j;

            for (j = 0; j <= num - 2; j = j + 2)
            {
                byte a = 0, b = 0;
                for (i = 7; i >= 0; i--)
                {
                    a = (byte)(a | output[i + 8 * j]);
                    if (i > 0)
                    { a = (byte)(a << 1); }
                    b = (byte)(b | output[i + 8 * (j + 1)]);
                    if (i > 0)
                    { b = (byte)(b << 1); }
                }
                outputB[j + 1] = a;
                outputB[j] = b;
            }
            return outputB;
        }
        public byte[] byteTObit(byte[] inputB, int num)
        {
            byte[] input = new byte[num * 8];
            int i, j;
            for (j = 0; j < num; j++)
            {
                for (i = 0; i <= 7; i++)
                {
                    input[8 * j + i] = (byte)(inputB[j] & byte.Parse(Math.Pow(2, i).ToString()));
                    if (i > 0) { input[8 * j + i] = (byte)(input[8 * j + i] >> i); }
                }
            }
            return input;
        }
        public short[] byteTOshort(byte[] Byte, int num)
        {
            short[] Short1 = new short[num];
            int j;
            for (j = 0; j < num; j++)
            {
                Short1[j] = (short)((Byte[2 * j + 0] << 8) + Byte[2 * j + 1]);
            }
            return Short1;
        }
        public short byteTOshortSingle(byte Byte1, byte Byte2)
        {
            short Short1 = new short();
                Short1 = (short)((Byte1 << 8) + Byte2);
            return Short1;
        }
        public byte[] shortTObyte(short[] Short1, int num)
        {
            byte[] Byte = new byte[num * 2];
            int j;
            for (j = 0; j < num; j++)
            {
                Byte[j * 2] = (byte)(Short1[j] >> 8);
                Byte[j * 2 + 1] = (byte)(Short1[j] & 255);
            }
            return Byte;
        }
        public int[] byteTOint(byte[] Byte, int num)
        {
            int[] int1 = new int[num];
            int j;
            for (j = 0; j < num; j++)
            {
                int1[j] = (int)((Byte[4 * j + 0] << 24) + (Byte[4 * j + 1] << 16) + (Byte[4 * j + 2] << 8) + Byte[4 * j + 3]);
            }
            return int1;
        }
        public int[] byteTOint_htol(byte[] Byte, int num)
        {
            int[] int1 = new int[num];
            int j;
            for (j = 0; j < num; j++)
            {
                int1[j] = (int)((Byte[4 * j + 2] << 24) + (Byte[4 * j + 3] << 16) + (Byte[4 * j + 0] << 8) + Byte[4 * j + 1]);
            }
            return int1;
        }
        public byte[] intTObyte(int[] int1, int num)
        {
            int i;
            byte[] Byte = new byte[num * 4];
            for (i = 0; i < num; i++)
            {
                Byte[0 + 4 * i] = (byte)(int1[i] >> 24);
                Byte[1 + 4 * i] = (byte)((int1[i] >> 16) & 255);
                Byte[2 + 4 * i] = (byte)((int1[i] >> 8) & 255);
                Byte[3 + 4 * i] = (byte)(int1[i] & 255);
            }
            return Byte;
        }
        public float[] byteTOfloat(byte[] Byte, int num)
        {
            float[] float1 = new float[num];
            byte[] Byte1 = new byte[Byte.Length];
            for (int i = 0; i < num; i++)
            {
                Byte1[4 * i + 3] = Byte[4 * i + 3];
                Byte1[4 * i + 2] = Byte[4 * i + 2];
                Byte1[4 * i + 1] = Byte[4 * i + 1];
                Byte1[4 * i + 0] = Byte[4 * i + 0];
            }
            for (int j = 0; j < num; j++)
            {
                float1[j] = BitConverter.ToSingle(Byte1, 4 * j);
            }
            return float1;
        }
        public float byteTOfloatSingle(byte[] Byte)
        {
            float float1 = new float();
            byte[] Byte1 = new byte[4];
            
                Byte1[ 3] = Byte[ 3];
                Byte1[ 2] = Byte[ 2];
                Byte1[ 1] = Byte[ 1];
                Byte1[ 0] = Byte[ 0];
            
                float1= BitConverter.ToSingle(Byte1, 0);
         
            return float1;
        }
        public byte[] floatTObyte(float[] float1, int num)
        {
            byte[] Byte1 = new byte[4];
            byte[] Byte2 = new byte[4];
            byte[] Byte = new byte[num * 4];
            int i;
            for (i = 0; i < num; i++)
            {
                Byte1 = BitConverter.GetBytes(float1[i]);
                Byte2[0] = Byte1[0];
                Byte2[1] = Byte1[1];
                Byte2[2] = Byte1[2];
                Byte2[3] = Byte1[3];
                Byte2.CopyTo(Byte, 4 * i);
            }
            return Byte;
        }
        public byte[] floatTObyteSingle(float float1)
        {
            byte[] Byte1 = new byte[4];
            byte[] Byte2 = new byte[4];
            byte[] Byte = new byte[4];
            
                Byte1 = BitConverter.GetBytes(float1);
                Byte2[0] = Byte1[0];
                Byte2[1] = Byte1[1];
                Byte2[2] = Byte1[2];
                Byte2[3] = Byte1[3];
                Byte2.CopyTo(Byte, 0 );
            
            return Byte;
        }
        public void highTolow(byte[] byte1, int num)
        {
            int HTOL;
            for (HTOL = 0; HTOL < num / 2; HTOL++)
            {
                byte HTOL0 = 0;
                byte HTOL1 = 0;
                HTOL0 = byte1[2 * HTOL];
                HTOL1 = byte1[2 * HTOL + 1];
                byte1[2 * HTOL] = HTOL1;
                byte1[2 * HTOL + 1] = HTOL0;
            }

        }
        public short highTolow_W(short Short1)
        {
            Byte BYTE1 = 0;
            Byte BYTE2 = 0;
            BYTE2 = (byte)(Short1 >> 8);
            BYTE1 = (byte)(Short1 & 255);
            Short1 = (short)((BYTE1 << 8) + BYTE2);
            return Short1;
        }
        public int highTolow_D(int INT)
        {
            byte[] Byte = new byte[4];
            Byte[0] = (byte)(INT >> 24);
            Byte[1] = (byte)((INT >> 16) & 255);
            Byte[2] = (byte)((INT >> 8) & 255);
            Byte[3] = (byte)(INT & 255);
            INT = (int)((Byte[2] << 24) + (Byte[3] << 16) + (Byte[0] << 8) + Byte[1]);
            return INT;
        }
        public float highTolow_F(float Float)
        {
            byte[] Byte1 = new byte[4];
            byte[] Byte2 = new byte[4];
            Byte1 = BitConverter.GetBytes(Float);
            Byte2[0] = Byte1[3];
            Byte2[1] = Byte1[2];
            Byte2[2] = Byte1[1];
            Byte2[3] = Byte1[0];
            Float = BitConverter.ToSingle(Byte2, 0);
            return Float;
        }
        public float highTolow_F1(float Float)
        {
            byte[] Byte1 = new byte[4];
            byte[] Byte2 = new byte[4];
            Byte1 = BitConverter.GetBytes(Float);
            Byte2[0] = Byte1[1];
            Byte2[1] = Byte1[0];
            Byte2[2] = Byte1[3];
            Byte2[3] = Byte1[2];
            Float = BitConverter.ToSingle(Byte2, 0);
            return Float;
        }

    }
}
