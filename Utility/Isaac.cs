using System;
using System.Collections.Generic;
using System.Text;

namespace Silvarea.Utility
{
    /**
------------------------------------------------------------------------------
Rand.java: By Bob Jenkins.  My random number generator, ISAAC.
      rand.init() -- initialize
      rand.val()  -- get a random value
MODIFIED:
      960327: Creation (addition of randinit, really)
      970719: use context, not global variables, for internal state
      980224: Translate to Java
------------------------------------------------------------------------------
*/
    public class Isaac
    {
        public const int SIZEL = 8;              /* log of size of rsl[] and mem[] */
        public const int SIZE = 1 << SIZEL;               /* size of rsl[] and mem[] */
        public const int MASK = (SIZE - 1) << 2;            /* for pseudorandom lookup */
        public int count;                           /* count through the results in rsl[] */
        public int[] rsl;                                /* the results given to the user */
        private int[] mem;                                   /* the internal state */
        private int a;                                              /* accumulator */
        private int b;                                          /* the last result */
        private int c;              /* counter, guarantees cycle is at least 2^^40 */


        /* no seed, equivalent to randinit(ctx,FALSE) in C */
        public Isaac()
        {
            mem = new int[SIZE];
            rsl = new int[SIZE];
            Init(false);
        }

        /* equivalent to randinit(ctx, TRUE) after putting seed in randctx in C */
        public Isaac(int[] seed)
        {
            mem = new int[SIZE];
            rsl = new int[SIZE];
            for (int i = 0; i < seed.Length; ++i)
            {
                rsl[i] = seed[i];
            }
            Init(true);
        }


        /* Generate 256 results.  This is a fast (not small) implementation. */
        public /*final*/ void IsaacGen()
        {
            int i, j, x, y;

            b += ++c;
            for (i = 0, j = SIZE / 2; i < SIZE / 2;)
            {
                x = mem[i];
                a ^= a << 13;
                a += mem[j++];
                mem[i] = y = mem[(x & MASK) >> 2] + a + b;
                rsl[i++] = b = mem[((y >> SIZEL) & MASK) >> 2] + x;

                x = mem[i];
                a ^= (int)((uint)a >> 6);
                a += mem[j++];
                mem[i] = y = mem[(x & MASK) >> 2] + a + b;
                rsl[i++] = b = mem[((y >> SIZEL) & MASK) >> 2] + x;

                x = mem[i];
                a ^= a << 2;
                a += mem[j++];
                mem[i] = y = mem[(x & MASK) >> 2] + a + b;
                rsl[i++] = b = mem[((y >> SIZEL) & MASK) >> 2] + x;

                x = mem[i];
                a ^= (int)((uint)a >> 16);
                a += mem[j++];
                mem[i] = y = mem[(x & MASK) >> 2] + a + b;
                rsl[i++] = b = mem[((y >> SIZEL) & MASK) >> 2] + x;
            }

            for (j = 0; j < SIZE / 2;)
            {
                x = mem[i];
                a ^= a << 13;
                a += mem[j++];
                mem[i] = y = mem[(x & MASK) >> 2] + a + b;
                rsl[i++] = b = mem[((y >> SIZEL) & MASK) >> 2] + x;

                x = mem[i];
                a ^= (int)((uint)a >> 6);
                a += mem[j++];
                mem[i] = y = mem[(x & MASK) >> 2] + a + b;
                rsl[i++] = b = mem[((y >> SIZEL) & MASK) >> 2] + x;

                x = mem[i];
                a ^= a << 2;
                a += mem[j++];
                mem[i] = y = mem[(x & MASK) >> 2] + a + b;
                rsl[i++] = b = mem[((y >> SIZEL) & MASK) >> 2] + x;

                x = mem[i];
                a ^= (int)((uint)a >> 16);
                a += mem[j++];
                mem[i] = y = mem[(x & MASK) >> 2] + a + b;
                rsl[i++] = b = mem[((y >> SIZEL) & MASK) >> 2] + x;
            }
        }


        /* initialize, or reinitialize, this instance of rand */
        public /*final*/ void Init(bool flag)
        {
            int i;
            int a, b, c, d, e, f, g, h;
            a = b = c = d = e = f = g = h = unchecked((int)0x9e3779b9);                        /* the golden ratio */

            for (i = 0; i < 4; ++i)
            {
                a ^= b << 11; d += a; b += c;
                b ^= (int)((uint)c >> 2); e += b; c += d;
                c ^= d << 8; f += c; d += e;
                d ^= (int)((uint)e >> 16); g += d; e += f;
                e ^= f << 10; h += e; f += g;
                f ^= (int)((uint)g >> 4); a += f; g += h;
                g ^= h << 8; b += g; h += a;
                h ^= (int)((uint)a >> 9); c += h; a += b;
            }

            for (i = 0; i < SIZE; i += 8)
            {              /* fill in mem[] with messy stuff */
                if (flag)
                {
                    a += rsl[i]; b += rsl[i + 1]; c += rsl[i + 2]; d += rsl[i + 3];
                    e += rsl[i + 4]; f += rsl[i + 5]; g += rsl[i + 6]; h += rsl[i + 7];
                }
                a ^= b << 11; d += a; b += c;
                b ^= (int)((uint)c >> 2); e += b; c += d;
                c ^= d << 8; f += c; d += e;
                d ^= (int)((uint)e >> 16); g += d; e += f;
                e ^= f << 10; h += e; f += g;
                f ^= (int)((uint)g >> 4); a += f; g += h;
                g ^= h << 8; b += g; h += a;
                h ^= (int)((uint)a >> 9); c += h; a += b;
                mem[i] = a; mem[i + 1] = b; mem[i + 2] = c; mem[i + 3] = d;
                mem[i + 4] = e; mem[i + 5] = f; mem[i + 6] = g; mem[i + 7] = h;
            }

            if (flag)
            {           /* second pass makes all of seed affect all of mem */
                for (i = 0; i < SIZE; i += 8)
                {
                    a += mem[i]; b += mem[i + 1]; c += mem[i + 2]; d += mem[i + 3];
                    e += mem[i + 4]; f += mem[i + 5]; g += mem[i + 6]; h += mem[i + 7];
                    a ^= b << 11; d += a; b += c;
                    b ^= (int)((uint)c >> 2); e += b; c += d;
                    c ^= d << 8; f += c; d += e;
                    d ^= (int)((uint)e >> 16); g += d; e += f;
                    e ^= f << 10; h += e; f += g;
                    f ^= (int)((uint)g >> 4); a += f; g += h;
                    g ^= h << 8; b += g; h += a;
                    h ^= (int)((uint)a >> 9); c += h; a += b;
                    mem[i] = a; mem[i + 1] = b; mem[i + 2] = c; mem[i + 3] = d;
                    mem[i + 4] = e; mem[i + 5] = f; mem[i + 6] = g; mem[i + 7] = h;
                }
            }

            IsaacGen();
            count = SIZE;
        }


        /* Call rand.val() to get a random value */
        public /*final*/ int val()
        {
            if (0 == count--)
            {
                IsaacGen();
                count = SIZE - 1;
            }
            return rsl[count];
        }

    }
}
