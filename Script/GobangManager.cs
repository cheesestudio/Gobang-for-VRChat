
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace CheeseGobang
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GobangManager : UdonSharpBehaviour
    {
        [SerializeField] public GameObject[] PieceTmp;
        [SerializeField]public GameObject[] TmpParent;
        private GameObject[] Pieces = new GameObject[15 * 15];      //15*15 = 225  half is 113 
        public TextMeshProUGUI winnerTmp;

        [SerializeField] private BitFieldUdonDemo bitFieldUdonDemo;
        private int flag = 0;
        private bool nextSetted = false;

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

        public int CheckForWin()
        {
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    int currentType = bitFieldUdonDemo.indexAt(i, j);
                    if (currentType != PieceTypeEmpty)
                    {
                        // 检查横向、纵向和两个对角线方向
                        if (CheckDirection(i, j, currentType, 1, 0) ||  // 横向
                            CheckDirection(i, j, currentType, 0, 1) ||  // 纵向
                            CheckDirection(i, j, currentType, 1, 1) ||  // \ 对角线
                            CheckDirection(i, j, currentType, 1, -1))   // / 对角线
                        {
                            return currentType; // 返回胜利方
                        }
                    }
                }
            }
            return PieceTypeEmpty; // 没有找到胜利条件
        }

        // 检查特定方向上的连续棋子
        private bool CheckDirection(int startX, int startY, int playerType, int dirX, int dirY)
        {
            int count = 1;

            count += CountInDirection(startX, startY, playerType, dirX, dirY);
            count += CountInDirection(startX, startY, playerType, -dirX, -dirY);

            return count >= 5; // 判断是否达到五子连珠
        }

        // 在指定方向上计数
        private int CountInDirection(int startX, int startY, int playerType, int dirX, int dirY)
        {
            int count = 0;
            int x = startX + dirX;
            int y = startY + dirY;

            while (IsInBounds(x, y) && bitFieldUdonDemo.indexAt(x, y) == playerType)
            {
                count++;
                x += dirX;
                y += dirY;
            }

            return count;
        }

        // 检查坐标是否在棋盘范围内
        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < 15 && y >= 0 && y < 15;
        }

        private void WinCheak()
        {
            int winner = CheckForWin();
            if (winner == PieceTypeWhite)
            {
                winnerTmp.text = "<color=#FFFFFF>White Win!</color>";
             
                nextSetted= true;
            }
            if (winner == PieceTypeBlack) 
            { 
                winnerTmp.text = "<color=#000000>Black Win!</color>"; 
                nextSetted= true;
            }
            if (winner == PieceTypeEmpty)
            {
                winnerTmp.text = "";
                nextSetted= false;
            }
        }
        public int Calculate()
        {
            int whiteNum = 0;
            int blackNum = 0;
            for (int i = 0;i <15;i++)
            {
                for(int j = 0;j < 15;j++)
                {
                    if(bitFieldUdonDemo.indexAt(i,j) == PieceTypeBlack) blackNum++;
                    if(bitFieldUdonDemo.indexAt(i,j) == PieceTypeWhite) whiteNum++;
                }
            }
            return whiteNum + blackNum;
        }

        public bool IsSqureEmpty(int x, int y)
        {
            if(x < 0 || x > 15 || y < 0 || y > 15)
            {
                Debug.Log("棋子位置有误:" + "x:"+ x + ",y:" + y);
            }
            return bitFieldUdonDemo.indexAt(x,y) == PieceTypeEmpty;
        }
        public void SetMap(int x, int y,int Type)
        {
            bitFieldUdonDemo.SetIndexAt(Type,x,y);

            flashMap();
        }
        public void InitGobang()
        {
            for(int i = 0; i < 225; i++)
            {
                Pieces[i] = Object.Instantiate(PieceTmp[0], PieceTmp[0].transform.position, Quaternion.identity, TmpParent[0].transform);
                PieceSetActive(i,true,PieceTypeEmpty);
                //Pieces[i].GetComponent<Piece>().PutBack();
            }
        }

        private void PieceSetActive(int i,bool Value,int Type)
        {
            if(Pieces[i].activeSelf != Value)
                Pieces[i].SetActive(Value);
            Piece pieceTmp = Pieces[i].GetComponent<Piece>();
            pieceTmp.enabled = Value;
            pieceTmp.TypeChange(Type);
        }
        private void PieceSetActive(int i, bool Value)
        {
            Pieces[i].SetActive(Value);
            Piece pieceTmp = Pieces[i].GetComponent<Piece>();
            pieceTmp.enabled = Value;
        }
        void Start()
        {
            InitGobang();
            //PieceSetActive(0, true, PieceTypeBlack);
            flashMap();

            //PieceSetActive(Offset,true);
        }

        public void flashMap()
        {
            flag = Calculate(); //奇黑偶白

            //for (int i = 0; i <= flag / 2; i++)
            //{
            //    PieceSetActive(i, true,PieceTypeBlack);
            //    if (i == flag / 2 && flag % 2 == 0) //保证偶数情况下,显示flag+1颗棋子
            //        break;
            //    PieceSetActive(i + Offset, true,PieceTypeWhite);
            //}
            nextSetted = false;
            WinCheak();
            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                {
                    Piece pieceTmp = Pieces[i*15+j].GetComponent<Piece>();

                    if (pieceTmp == null)
                        Debug.Log("获取组件失败");

                    switch (bitFieldUdonDemo.indexAt(i, j))
                    {
                        case PieceTypeBlack:
                            PieceSetActive(i * 15 + j, true);
                            pieceTmp.SetPiece(i, j, PieceTypeBlack);
                            break;
                        case PieceTypeWhite:
                            PieceSetActive(i * 15 + j, true);
                            pieceTmp.SetPiece(i, j, PieceTypeWhite);
                            break;
                        case PieceTypeEmpty:
                            if (nextSetted)
                            {
                                PieceSetActive(i * 15 + j, false);
                                break;
                            }
                            nextSetted = true;
                            PieceSetActive(i * 15 + j, true,flag % 2 == 1 ? PieceTypeWhite : PieceTypeBlack);
                            pieceTmp.PutBack();
                            break;
                    }
                }
        }
        public override void OnDeserialization()
        {
            flashMap();
        }
    }
}