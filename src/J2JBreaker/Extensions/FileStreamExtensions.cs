using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2JBreaker.Extensions
{
    internal static class FileStreamExtensions
    {
        internal static long Find(this FileStream fileStream, long offset, byte[] bytes)
        {
            if (fileStream == null)
            {
                throw new NullReferenceException("The file stream is null.");
            }

            if (bytes == null)
            {
                throw new NullReferenceException("The bytes to find is null.");
            }

            if (fileStream.Length < bytes.Length)
            {
                throw new IndexOutOfRangeException("The length of the array to be retrieved must be smaller than the length of the file stream.");
            }

            fileStream.Position = offset;

            bool result = false;

            for (long i=offset; i<fileStream.Length; i++)
            {
                if (fileStream.ReadByte() == bytes[0])
                {
                    long currentOffset = fileStream.Position;

                    // Copys bytes.
                    byte[] buffer = new byte[bytes.Length];
                    fileStream.Position -= 1;
                    int readBytes = fileStream.Read(buffer, 0, bytes.Length);

                    if (readBytes != bytes.Length)
                    {
                        fileStream.Position = currentOffset;
                        continue;
                    }

                    // Compares bytes.
                    result = true;

                    for (int j=0; j<bytes.Length; j++)
                    {
                        if (buffer[j] != bytes[j])
                        {
                            result = false;
                            break;
                        }
                    }

                    // Checks the result.
                    if (!result)
                    {
                        fileStream.Position = currentOffset;
                        continue;
                    }

                    return i;
                }
            }

            return -1;
        }

        internal static async Task<long> FindAsync(this FileStream fileStream, long offset, byte[] bytes)
        {
            var task = Task.Factory.StartNew(() =>
            {
                return Find(fileStream, offset, bytes);
            });

            return await task;
        }

        internal static List<long> FindAll(this FileStream fileStream, long offset, byte[] bytes)
        {
            if (fileStream == null)
            {
                throw new NullReferenceException("The file stream is null.");
            }

            if (bytes == null)
            {
                throw new NullReferenceException("The bytes to find is null.");
            }

            if (fileStream.Length < bytes.Length)
            {
                throw new IndexOutOfRangeException("The length of the array to be retrieved must be smaller than the length of the file stream.");
            }

            List<long> result = new List<long>();

            long currentOffset = offset;

            while (true)
            {
                long findingResult = fileStream.Find(currentOffset, bytes);

                if (findingResult != -1)
                {
                    result.Add(findingResult);

                    currentOffset = findingResult + bytes.Length;
                }
                else
                {
                    return result;
                }
            }
        }

        internal static async Task<List<long>> FindAllAsync(this FileStream fileStream, long offset, byte[] bytes)
        {
            var task = Task.Factory.StartNew(() =>
            {
                return FindAll(fileStream, offset, bytes);
            });

            return await task;
        }
    }
}
