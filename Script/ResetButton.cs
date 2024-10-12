
using CheeseGobang;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
namespace CheeseGobang 
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ResetButton : UdonSharpBehaviour
    {
        public BitFieldUdonDemo bit;

        public override void Interact()
        {
            bit.ResetBytesArray();
        }
        void Start()
        {

        }
    }
}