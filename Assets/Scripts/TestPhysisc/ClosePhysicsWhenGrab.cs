/************************************************************************************
Copyright : Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Your use of this SDK or tool is subject to the Oculus SDK License Agreement, available at
https://developer.oculus.com/licenses/oculussdk/

Unless required by applicable law or agreed to in writing, the Utilities SDK distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
ANY KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.
************************************************************************************/

using Oculus.Interaction.HandPosing;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Oculus.Interaction.Samples
{
    public class ClosePhysicsWhenGrab : MonoBehaviour
    {
        [SerializeField]
        private HandGrabInteractor _handGrabInteractor;
        [SerializeField]
        private PhysicsHand _handPhysics;

        private ConfigurableJoint prev_joint;

        //   [SerializeField]
        //   private HandVisual _handVisual;

        protected virtual void Start()
        {
          //  Assert.IsNotNull(_handVisual);
        }

        protected virtual void Update()
        {
            GameObject volume = null;

            if (_handGrabInteractor.State == InteractorState.Select)
            {
                volume = _handGrabInteractor.SelectedInteractable?.gameObject;
            }

            if (volume)
            {
                _handPhysics.DisablePhysics();
              /*  if (prev_joint == null && volume.GetComponent<ShouldCreateJoint>())
                {
                    CreateConfigurableJoint(volume.transform.parent.gameObject,_handPhysics.GetComponent<Rigidbody>(),volume.GetComponent<ShouldCreateJoint>().offset);
                }*/
            }
            else
            {
            //    DestroyJoint();
                _handPhysics.EnablePhysics();
               // _handVisual.ForceOffVisibility = false;
            }
        }

        #region Inject

        public void InjectAll(HandGrabInteractor handGrabInteractor,
             HandVisual handVisual)
        {
            InjectHandGrabInteractor(handGrabInteractor);
      //      InjectHandVisual(handVisual);
        }
        private void InjectHandGrabInteractor(HandGrabInteractor handGrabInteractor)
        {
            _handGrabInteractor = handGrabInteractor;
        }

   /*     private void InjectHandVisual(HandVisual handVisual)
        {
            _handVisual = handVisual;
        }*/

        private void CreateConfigurableJoint(GameObject target,Rigidbody attachedBody,Vector3 offset,byte type = 0)
        {

            ConfigurableJoint conj = target.AddComponent<ConfigurableJoint>();
            if(type == 0)
            {
                conj.connectedBody = attachedBody;
                conj.anchor = offset;
                conj.autoConfigureConnectedAnchor = false;
                conj.connectedAnchor = new Vector3(0, 0, 0);
                conj.xMotion = ConfigurableJointMotion.Limited;
                conj.yMotion = ConfigurableJointMotion.Locked;
                conj.zMotion = ConfigurableJointMotion.Limited;
                conj.angularXMotion = ConfigurableJointMotion.Free;
                conj.angularYMotion = ConfigurableJointMotion.Free;
                conj.angularZMotion = ConfigurableJointMotion.Free;
                conj.rotationDriveMode = RotationDriveMode.Slerp;
                JointDrive jd1 = new JointDrive();
                jd1.positionSpring = 10000;
                jd1.positionDamper = 10;
                jd1.maximumForce = 300000;
                conj.xDrive = jd1;
                conj.yDrive = jd1;
                conj.zDrive = jd1;
                JointDrive jd = new JointDrive();
                jd.positionSpring =10000;
                jd.positionDamper = 25;
                jd.maximumForce = 1000;
                conj.slerpDrive = jd;
                conj.configuredInWorldSpace = true;
            }
            prev_joint = conj;
        }

        private void DestroyJoint()
        {
            if(prev_joint != null)
            {
                Debug.Log("+++--- Destroy joint ---+++");
                Destroy(prev_joint);
            }
        }


        #endregion
    }
}
