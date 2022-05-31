using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Oculus.Interaction
{
    public class HandSnapper : MonoBehaviour
    {
        [SerializeField] ConfigurableJoint _joint;
        public Transform _snappingHand;
        private bool _takeSnap = false;
        public void SnapToHand()
        {
            _takeSnap = true;
        }
        public void Unsnap()
        {
            _takeSnap = false;
        }
        private void Update()
        {
            if (_takeSnap)
            {
                _joint.connectedAnchor = _snappingHand.transform.position;
                _joint.targetRotation = _snappingHand.transform.rotation;
            }
        }

        public void GetSnappingHand()
        {

        }
    }
}
