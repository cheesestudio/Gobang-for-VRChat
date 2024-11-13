
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace CheeseGobang
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Piece : UdonSharpBehaviour
    {
        [SerializeField] GameObject SelectedMark;
        [SerializeField] MeshRenderer Renderer;
        [SerializeField] Material WhiteMaterial;
        [SerializeField] Material BlackMaterial;

        [SerializeField] GobangManager Gobang;
        [SerializeField] Vector3 initalPosition;
        [SerializeField] Vector3 initalPositionb;
        public Vector3 SnapPositionScale = new Vector3(0.065f, 1000, 0.065f);
        private const float HalfSquareSize = 0.455f;

        private bool isPicked=false;
        private VRC_Pickup pickup;

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

        public int Type=PieceTypeEmpty;

        public override void OnDrop()
        {
            isPicked = false;
            SelectedMark.SetActive(false);

            Vector3 droppedPosition = transform.localPosition;
            Vector3 droppedRotation = new Vector3(0,0,0);
            droppedPosition.x = Mathf.Round(droppedPosition.x / SnapPositionScale.x) * SnapPositionScale.x;
            droppedPosition.y = 0.0564f;//Mathf.Round(droppedPosition.y / SnapPositionScale.y) * SnapPositionScale.y;
            droppedPosition.z = Mathf.Round(droppedPosition.z / SnapPositionScale.z) * SnapPositionScale.z;

            int mapx = (int)((droppedPosition.x + HalfSquareSize) / SnapPositionScale.x);
            int mapy = (int)((droppedPosition.z + HalfSquareSize) / SnapPositionScale.z);
            //判断是否越界 和 重复
            if (Mathf.Abs(droppedPosition.x) > HalfSquareSize || Mathf.Abs(droppedPosition.z) > HalfSquareSize || !Gobang.IsSqureEmpty(mapx,mapy))
            {
                PutBack();
                return;
            }

            //成功放置
            
            //Debug.Log("棋子位置:" + "x:" + mapx + ",y:" + mapy + "type:" + Type);
            Gobang.SetMap(mapx,mapy,Type);

            //pickup.pickupable = false;
           // Type = PieceTypeEmpty;
           // transform.localPosition = droppedPosition;
            //transform.localRotation = Quaternion.Euler(droppedRotation);

        }

        public void SetPiece(int x, int y, int type)
        {
            if (type == PieceTypeEmpty)
                Debug.Log("不可放置空白类型棋子");
            float px = x * SnapPositionScale.x - HalfSquareSize;
            float py = y * SnapPositionScale.z - HalfSquareSize;
            //Debug.Log("x:"+x+"y:"+y+"转换坐标"+"px:"+px+"py:"+py);
            Vector3 Position = new Vector3(px, 0.0564f, py);
            transform.localPosition = Position;
            TypeChange(type);
            pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
            pickup.pickupable = false;
        }
        public override void OnPickup()
        {
            isPicked = true;
        }

        private void Update()
        {
            if (!isPicked)
            {
                return;
            }
            Vector3 droppedPosition = transform.localPosition;
            droppedPosition.x = Mathf.Round(droppedPosition.x / SnapPositionScale.x) * SnapPositionScale.x;
            droppedPosition.y = 0.0564f;//Mathf.Round(droppedPosition.y / SnapPositionScale.y) * SnapPositionScale.y;
            droppedPosition.z = Mathf.Round(droppedPosition.z / SnapPositionScale.z) * SnapPositionScale.z;

            int mapx = (int)((droppedPosition.x + HalfSquareSize) / SnapPositionScale.x);
            int mapy = (int)((droppedPosition.z + HalfSquareSize) / SnapPositionScale.z);

            if (Mathf.Abs(droppedPosition.x) < HalfSquareSize && Mathf.Abs(droppedPosition.z) < HalfSquareSize && Gobang.IsSqureEmpty(mapx, mapy))
            {

                SelectedMark.transform.localPosition = droppedPosition;
                SelectedMark.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                SelectedMark.SetActive(true);
            }
            else 
            {
                SelectedMark.SetActive(false);
            }
        }
        void Start()
        {
            //initalPosition = transform.localPosition;
            pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        }

        public void PutBack()
        {
            pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
            pickup.Drop();
            pickup.pickupable = false;
            isPicked = false;
            transform.localPosition = Type==PieceTypeWhite?initalPosition:initalPositionb;
            transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
            if(SelectedMark.activeSelf)
                SelectedMark.SetActive(false);

            pickup.pickupable = true;
        }

        public void TypeChange(int type)
        {
            Type = type;
            if (type == PieceTypeWhite) Renderer.material = WhiteMaterial;
            if (type == PieceTypeBlack) Renderer.material = BlackMaterial;

            //if (type == PieceTypeEmpty)  gameObject.SetActive(false);
        }
    }
}