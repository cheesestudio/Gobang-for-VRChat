
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace CheeseGobang
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class BitFieldUdonDemo : UdonSharpBehaviour
    {
        [SerializeField]GobangManager gobangManager;

        /// <summary>
        /// 15 * 15 * 2 = 450 bits
        /// 450 / 8.0 = 56.25
        /// 使用 57 bytes
        /// </summary>
        [UdonSynced]public byte[] BytesArray = new byte[57];

        /// <summary>
        /// 空
        /// </summary>
        const int PieceTypeEmpty = 0;
        /// <summary>
        /// 白子
        /// </summary>
        const int PieceTypeWhite = 1;
        /// <summary>
        /// 黑子
        /// </summary>
        const int PieceTypeBlack = 2;

        void Start()
        {
            InitializeBytesArray();
        }

        private void InitializeBytesArray()
        {
            if(!Networking.IsOwner(gameObject)) return;
            for (int i = 0; i < BytesArray.Length; i++)
            {
                BytesArray[i] = PieceTypeEmpty; 
            }
            RequestSerialization();
        }

        public void ResetBytesArray()
        { 
            if(!Networking.IsOwner(gameObject))
                Networking.SetOwner(Networking.LocalPlayer,gameObject);

            for(int i = 0;i < BytesArray.Length;i++)
            {
                BytesArray[i] = PieceTypeEmpty;
            }
            gobangManager.flashMap();
            RequestSerialization();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            RequestSerialization();
        }
        /// <summary>
        /// Pices[x][y]
        /// X 行 Y 列
        /// 计数从 0 开始
        /// X, Y 范围 [0, 14]
        /// </summary>
        public int indexAt(int x, int y)
        {
            // 计算平铺成一维数组后第几个棋子
            int indexOfPiece = 15 * x + y;

            // 每个棋子占用 2bits, 因此每个 byte 存储 4 个棋子
            // 计算该棋子存储在第几个 byte, 获取这个 byte 的值
            byte SelectedByte = BytesArray[indexOfPiece / 4];

            // 计算在这个 byte 的偏移量
            switch (indexOfPiece % 4)
            {
                case 0:
                    break;                  // 0b 0000 0011
                case 1:
                    // 无符号右移
                    SelectedByte >>= 2;     // 0b 0000 1100
                    break;
                case 2:
                    SelectedByte >>= 4;     // 0b 0011 0000
                    break;
                case 3:
                    SelectedByte >>= 6;     // 0b 1100 0000
                    break;
            }

            // 只要最后两位, 0b 0000 0011 = 0x03
            SelectedByte &= 0x03;

            switch (SelectedByte)
            {
                case 0:
                    return PieceTypeEmpty;
                case 1:
                    return PieceTypeWhite;
                default:
                    return PieceTypeBlack;
            }
        }

        public void SetIndexAt(int pieceType, int x, int y)
        {
            if(!Networking.IsOwner(gameObject))
                Networking.SetOwner(Networking.LocalPlayer,gameObject);

            // 计算平铺成一维数组后第几个棋子
            int indexOfPiece = 15 * x + y;

            // 每个棋子占用 2bits, 因此每个 byte 存储 4 个棋子
            // 计算该棋子存储在第几个 byte
            int index = indexOfPiece / 4;

            // 保险起见清空不需要的位
            pieceType &= 0x03;  // 0b 0000 0011

            // 获取这个 byte 的值
            byte SelectedByte = BytesArray[indexOfPiece / 4];

            // 计算在这个 byte 的偏移量, 清空该位置且设置对应的值
            switch (indexOfPiece % 4)
            {
                case 0:
                    SelectedByte &= 0xFC;  // 0b 1111 1100
                    SelectedByte |= (byte)pieceType;
                    break;
                case 1:
                    SelectedByte &= 0xF3;  // 0b 1111 0011
                    SelectedByte |= (byte)(pieceType << 2);
                    break;
                case 2:
                    SelectedByte &= 0xCF;  // 0b 1100 1111
                    SelectedByte |= (byte)(pieceType << 4);
                    break;
                case 3:
                    SelectedByte &= 0x3F;  // 0b 0011 1111
                    SelectedByte |= (byte)(pieceType << 6);
                    break;
            }

            BytesArray[index] = SelectedByte;

            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            gobangManager.flashMap();
        }
    }
}