using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace hackme
{
    class Program
    {

        const int PROCESS_VM_WRITE = 0x20;
        const int PROCESS_VM_OPERATION = 0x8;
        const int PROCESS_VM_READ = 0x10;
        const int GAME_POINTER_OFFSET = 0x1C2D0;
        const int PLAYER_HEALTH_OFFSET = 0x4;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(
            int hProcess,
            int lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(
            int hProcess,
            int lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            ref int lpNumberOfBytesWritten);

        static void Main(string[] args)
        {
            var permissions = PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION;
            var process = Process.GetProcessesByName("testgame")[0];
            var processHandle = OpenProcess(permissions, false, process.Id);

            var bytes = 0;
            var buffer = new byte[4];

            var gamePointerAddress = process.MainModule.BaseAddress + GAME_POINTER_OFFSET;

            ReadProcessMemory((int)processHandle, (int)gamePointerAddress, buffer, buffer.Length, ref bytes);
            var playerPointerAddress = BitConverter.ToInt32(buffer, 0);

            ReadProcessMemory((int)processHandle, playerPointerAddress, buffer, buffer.Length, ref bytes);
            var playerAddress = BitConverter.ToInt32(buffer, 0);

            var playerHealthAddress = playerAddress + 4;
            ReadProcessMemory((int)processHandle, playerHealthAddress, buffer, buffer.Length, ref bytes);
            var playerHealth = BitConverter.ToInt32(buffer, 0);

            Console.WriteLine($"Player Health: {playerHealth}");

            Console.Write("Change Player's health to?: ");
            var newHealthValue = Convert.ToInt32(Console.ReadLine());

            // convert int to byte array
            buffer = BitConverter.GetBytes(newHealthValue);

            // overwrite Player's current health value with new value
            WriteProcessMemory((int)processHandle, playerHealthAddress, buffer, buffer.Length, ref bytes);

            // confirm that the bytes were written
            ReadProcessMemory((int)processHandle, playerHealthAddress, buffer, buffer.Length, ref bytes);
            Console.WriteLine($"\nPlayer's health is now: {BitConverter.ToInt32(buffer, 0)}");

            Console.ReadLine();
        }
    }
}
