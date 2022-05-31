/************************************************************************************
Copyright : Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Your use of this SDK or tool is subject to the Oculus SDK License Agreement, available at
https://developer.oculus.com/licenses/oculussdk/

Unless required by applicable law or agreed to in writing, the Utilities SDK distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
ANY KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.
************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Oculus.Interaction
{
    public class PhysicsGrabbable : MonoBehaviour
    {
        [SerializeField]
        private Grabbable _grabbable;

        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        [Tooltip("If enabled, the object's mass will scale appropriately as the scale of the object changes.")]
        private bool _scaleMassWithSize = true;

        [SerializeField]
        private bool _disablePhysics = true;

        [SerializeField]
        private ConfigurableJoint connectedJoint;

        private bool _savedIsKinematicState = false;
        private bool _isBeingTransformed = false;
        private Vector3 _initialScale;
        private bool _hasPendingForce;
        private Vector3 _linearVelocity;
        private Vector3 _angularVelocity;

        [Header("Physics Joint Settings")]
        /// <summary>
        /// How much Spring Force to apply to the joint when something comes in contact with the grabbable
        /// A higher Spring Force will make the Grabbable more rigid
        /// </summary>
        [Tooltip("A higher Spring Force will make the Grabbable more rigid")]
        public float CollisionSpring = 3000;

        /// <summary>
        /// How much Slerp Force to apply to the joint when something is in contact with the grabbable
        /// </summary>
        [Tooltip("How much Slerp Force to apply to the joint when something is in contact with the grabbable")]
        public float CollisionSlerp = 500;

        [Tooltip("How to restrict the Configurable Joint's xMotion when colliding with an object. Position can be free, completely locked, or limited.")]
        public ConfigurableJointMotion CollisionLinearMotionX = ConfigurableJointMotion.Free;

        [Tooltip("How to restrict the Configurable Joint's yMotion when colliding with an object. Position can be free, completely locked, or limited.")]
        public ConfigurableJointMotion CollisionLinearMotionY = ConfigurableJointMotion.Free;

        [Tooltip("How to restrict the Configurable Joint's zMotion when colliding with an object. Position can be free, completely locked, or limited.")]
        public ConfigurableJointMotion CollisionLinearMotionZ = ConfigurableJointMotion.Free;

        [Tooltip("Restrict the rotation around the X axes to be Free, completely Locked, or Limited when colliding with an object.")]
        public ConfigurableJointMotion CollisionAngularMotionX = ConfigurableJointMotion.Free;

        [Tooltip("Restrict the rotation around the Y axes to be Free, completely Locked, or Limited when colliding with an object.")]
        public ConfigurableJointMotion CollisionAngularMotionY = ConfigurableJointMotion.Free;

        [Tooltip("Restrict the rotation around Z axes to be Free, completely Locked, or Limited when colliding with an object.")]
        public ConfigurableJointMotion CollisionAngularMotionZ = ConfigurableJointMotion.Free;


        [Tooltip("If true, the object's velocity will be adjusted to match the grabber. This is in addition to any forces added by the configurable joint.")]
        public bool ApplyCorrectiveForce = true;

        [Header("Velocity Grab Settings")]
        public float MoveVelocityForce = 3000f;
        public float MoveAngularVelocityForce = 90f;

        protected bool _started = false;

 public Vector3 GrabPositionOffset;
        private bool isBeingHold;

        public event Action<Vector3, Vector3> WhenVelocitiesApplied = delegate { };

        private void Reset()
        {
            _grabbable = this.GetComponent<Grabbable>();
            _rigidbody = this.GetComponent<Rigidbody>();
        }

        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            Assert.IsNotNull(_grabbable);
            Assert.IsNotNull(_rigidbody);
            this.EndStart(ref _started);
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                _grabbable.WhenGrabbableUpdated += HandleGrabbableUpdated;
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                _grabbable.WhenGrabbableUpdated -= HandleGrabbableUpdated;
            }
        }

        private void HandleGrabbableUpdated(GrabbableArgs args)
        {
            //Debug.Log(gameObject.name + " received "+args.GrabbableEvent.ToString());
            switch (args.GrabbableEvent)
            {
                case GrabbableEvent.Add:
                    if (_grabbable.GrabPointsCount == 1 && !_isBeingTransformed)
                    {
                        GrabPositionOffset = _grabbable.GrabPoints[0].position;
                        isBeingHold = true;
                        if (_disablePhysics)
                        {
                            DisablePhysics();
                            return;
                        }
                    }

                    break;
                case GrabbableEvent.Remove:
                    if (_grabbable.GrabPointsCount == 0)
                    {
                        if (_disablePhysics)
                        {
                            ReenablePhysics();
                        }
                        if (connectedJoint != null)
                        {
                            isBeingHold = false;
                            connectedJoint.connectedBody = null;
                            connectedJoint = null;
                        }
                    }

                    break;
            }
        }

        public void SetConnectedJoint(ConfigurableJoint joint)
        {
            connectedJoint = joint;
            connectedJoint.connectedBody = _rigidbody;
            Debug.Log(connectedJoint.gameObject.name + " connected to " + _rigidbody.gameObject.name);
        }

        private void DisablePhysics()
        {
            _isBeingTransformed = true;
            CachePhysicsState();
            _rigidbody.isKinematic = true;
        }

        private void ReenablePhysics()
        {
            _isBeingTransformed = false;
            // update the mass based on the scale change
            if (_scaleMassWithSize)
            {
                float initialScaledVolume = _initialScale.x * _initialScale.y * _initialScale.z;

                Vector3 currentScale = _rigidbody.transform.localScale;
                float currentScaledVolume = currentScale.x * currentScale.y * currentScale.z;

                float changeInMassFactor = currentScaledVolume / initialScaledVolume;
                _rigidbody.mass *= changeInMassFactor;
            }

            // revert the original kinematic state
            _rigidbody.isKinematic = _savedIsKinematicState;
        }

        public void ApplyVelocities(Vector3 linearVelocity, Vector3 angularVelocity)
        {
            _hasPendingForce = true;
            _linearVelocity = linearVelocity;
            _angularVelocity = angularVelocity;
        }

        private void FixedUpdate()
        {
            if (_hasPendingForce)
            {
                _hasPendingForce = false;
                _rigidbody.AddForce(_linearVelocity, ForceMode.VelocityChange);
                _rigidbody.AddTorque(_angularVelocity, ForceMode.VelocityChange);
                WhenVelocitiesApplied(_linearVelocity, _angularVelocity);
            }
            if (isBeingHold)
            {
                UpdatePhysicsJoints();
            }
        }

        private void CachePhysicsState()
        {
            _savedIsKinematicState = _rigidbody.isKinematic;
            _initialScale = _rigidbody.transform.localScale;
        }

        public virtual void UpdatePhysicsJoints()
        {

            // Bail if no joint connected
            if (connectedJoint == null || _rigidbody == null)
            {
                return;
            }

            // Set to continuous dynamic while being held
            if (_rigidbody.isKinematic)
            {
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
            else
            {
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            // Update Joint poisition in real time
            //if (GrabMechanic == GrabType.Snap)
           // {
                connectedJoint.anchor = Vector3.zero;
                connectedJoint.connectedAnchor = GrabPositionOffset;
           // }
           /*
            // Check if something is requesting a springy joint
            // For example, a gun may wish to make the joint springy in order to apply recoil to a weapon via AddForce
            bool forceSpring = Time.time < requestSpringTime;

            // Only snap to a _rigidbody grip if it's been a short delay after our last collision
            // This prevents the joint from rapidly becoming stiff / springy which will cause jittery behaviour
            bool afterCollision = collisions.Count == 0 && lastNoCollisionSeconds >= 0.1f;*/

            // Nothing touching it so we can stick to hand _rigidbodyly
            // Two-Handed weapons currently react much more smoothly if the joint is _rigidbody, due to how the LookAt system works
            //if ((BeingHeldWithTwoHands || afterCollision) && !forceSpring)
          //  {
                // Lock Angular, XYZ Motion
                // Make joint very _rigidbody
                connectedJoint.rotationDriveMode = RotationDriveMode.Slerp;
            connectedJoint.xMotion = CollisionLinearMotionX;
            connectedJoint.yMotion = CollisionLinearMotionY;
            connectedJoint.zMotion = CollisionLinearMotionZ;
            connectedJoint.angularXMotion = CollisionAngularMotionX;
            connectedJoint.angularYMotion = CollisionAngularMotionY;
            connectedJoint.angularZMotion = CollisionAngularMotionZ;
            /*  connectedJoint.xMotion = ConfigurableJointMotion.Limited;
              connectedJoint.yMotion = ConfigurableJointMotion.Limited;
              connectedJoint.zMotion = ConfigurableJointMotion.Limited;
              connectedJoint.angularXMotion = ConfigurableJointMotion.Limited;
              connectedJoint.angularYMotion = ConfigurableJointMotion.Limited;
              connectedJoint.angularZMotion = ConfigurableJointMotion.Limited;*/

            SoftJointLimit sjl = connectedJoint.linearLimit;
                sjl.limit = 15f;

                SoftJointLimitSpring sjlsp = connectedJoint.linearLimitSpring;
                sjlsp.spring = 3000;
                sjlsp.damper = 10f;

                // Set X,Y, and Z drive to our values
                // Set X,Y, and Z drive to our values
                setPositionSpring(CollisionSpring, 10f);

                // Slerp drive used for rotation
                setSlerpDrive(CollisionSlerp, 10f);

                // Adjust item velocity. This smooths out forces while becoming _rigidbody
                if (ApplyCorrectiveForce)
                {
                    moveWithVelocity();
                }
            //}
           /* else
            {
                // Make Springy
                connectedJoint.rotationDriveMode = RotationDriveMode.Slerp;
                connectedJoint.xMotion = CollisionLinearMotionX;
                connectedJoint.yMotion = CollisionLinearMotionY;
                connectedJoint.zMotion = CollisionLinearMotionZ;
                connectedJoint.angularXMotion = CollisionAngularMotionX;
                connectedJoint.angularYMotion = CollisionAngularMotionY;
                connectedJoint.angularZMotion = CollisionAngularMotionZ;

                SoftJointLimitSpring sp = connectedJoint.linearLimitSpring;
                sp.spring = 5000;
                sp.damper = 5;

                // Set X,Y, and Z drive to our values
                setPositionSpring(CollisionSpring, 5f);

                // Slerp drive used for rotation
                setSlerpDrive(CollisionSlerp, 5f);
            }

            if (BeingHeldWithTwoHands && SecondaryLookAtTransform != null)
            {
                connectedJoint.angularXMotion = ConfigurableJointMotion.Free;

                setSlerpDrive(1000f, 2f);
                connectedJoint.angularYMotion = ConfigurableJointMotion.Limited;


                connectedJoint.angularZMotion = ConfigurableJointMotion.Limited;

               /* if (TwoHandedRotation == TwoHandedRotationType.LookAtSecondary)
                {
                    checkSecondaryLook();
                }
            }*/
        }

        void moveWithVelocity()
        {

            if (_rigidbody == null || connectedJoint == null) { return; }

            Vector3 destination = connectedJoint.transform.position;

            float distance = Vector3.Distance(transform.position, destination);

            if (distance > 0.002f)
            {
                Vector3 positionDelta = destination - transform.position;

                // Move towards hand using velocity
                _rigidbody.velocity = Vector3.MoveTowards(_rigidbody.velocity, (positionDelta * MoveVelocityForce) * Time.fixedDeltaTime, 1f);
            }
            else
            {
                // Very close - just move object right where it needs to be and set velocity to 0 so it doesn't overshoot
                _rigidbody.MovePosition(destination);
                _rigidbody.velocity = Vector3.zero;
            }
        }

        void setPositionSpring(float spring, float damper)
        {

            if (connectedJoint == null)
            {
                return;
            }

            JointDrive xDrive = connectedJoint.xDrive;
            xDrive.positionSpring = spring;
            xDrive.positionDamper = damper;
            connectedJoint.xDrive = xDrive;

            JointDrive yDrive = connectedJoint.yDrive;
            yDrive.positionSpring = spring;
            yDrive.positionDamper = damper;
            connectedJoint.yDrive = yDrive;

            JointDrive zDrive = connectedJoint.zDrive;
            zDrive.positionSpring = spring;
            zDrive.positionDamper = damper;
            connectedJoint.zDrive = zDrive;
        }

        void setSlerpDrive(float slerp, float damper)
        {
            if (connectedJoint)
            {
                JointDrive slerpDrive = connectedJoint.slerpDrive;
                slerpDrive.positionSpring = slerp;
                slerpDrive.positionDamper = damper;
                connectedJoint.slerpDrive = slerpDrive;
            }
        }


        #region Inject

        public void InjectAllPhysicsGrabbable(Grabbable grabbable, Rigidbody rigidbody)
        {
            InjectGrabbable(grabbable);
            InjectRigidbody(rigidbody);
        }

        public void InjectGrabbable(Grabbable grabbable)
        {
            _grabbable = grabbable;
        }

        public void InjectRigidbody(Rigidbody rigidbody)
        {
            _rigidbody = rigidbody;
        }

        public void InjectOptionalScaleMassWithSize(bool scaleMassWithSize)
        {
            _scaleMassWithSize = scaleMassWithSize;
        }

        #endregion
    }
}
