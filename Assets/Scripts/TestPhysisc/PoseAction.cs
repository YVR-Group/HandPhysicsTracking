using Oculus.Interaction;
using Oculus.Interaction.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PoseAction : MonoBehaviour
{
    [SerializeField] private ActiveStateSelector[] _poses;
    [SerializeField] private GameObject[] _poseActiveVisualPrefab;
    [SerializeField] private UnityEvent[] _events;

    private GameObject[] _poseActiveVisuals;

    private void Start()
    {
        _poseActiveVisuals = new GameObject[_poses.Length];
        for (int i = 0; i < _poses.Length; i++)
        {
            if (_poseActiveVisualPrefab[i] != null)
            {
                _poseActiveVisuals[i] = Instantiate(_poseActiveVisualPrefab[i]);
                // _poseActiveVisuals[i].GetComponentInChildren<TextMeshPro>().text = _poses[i].name;
                //  _poseActiveVisuals[i].GetComponentInChildren<ParticleSystemRenderer>().material = _onSelectIcons[i];
                _poseActiveVisuals[i].SetActive(false);
            }

            int poseNumber = i;
            _poses[i].WhenSelected += () => ShowVisuals(poseNumber);
            _poses[i].WhenUnselected += () => HideVisuals(poseNumber);
        }
    }
    private void ShowVisuals(int poseNumber)
    {
        if (_poseActiveVisuals[poseNumber] != null)
        {
            var centerEyePos = FindObjectOfType<OVRCameraRig>().centerEyeAnchor.position;
            Vector3 spawnSpot = centerEyePos + FindObjectOfType<OVRCameraRig>().centerEyeAnchor.forward;

            _poseActiveVisuals[poseNumber].transform.position = spawnSpot;
            _poseActiveVisuals[poseNumber].transform.LookAt(2 * _poseActiveVisuals[poseNumber].transform.position - centerEyePos);

            var hands = _poses[poseNumber].GetComponents<HandRef>();
            Vector3 visualsPos = Vector3.zero;
            foreach (var hand in hands)
            {
                hand.GetRootPose(out Pose wristPose);
                Vector3 forward = hand.Handedness == Handedness.Left ? wristPose.right : -wristPose.right;
                visualsPos += wristPose.position + forward * .15f + Vector3.up * .02f;
            }
            _poseActiveVisuals[poseNumber].transform.position = visualsPos / hands.Length;
            _poseActiveVisuals[poseNumber].gameObject.SetActive(true);
        }
        if(_events[poseNumber] != null)
        {
            _events[poseNumber].Invoke();
        }
    }

    private void HideVisuals(int poseNumber)
    {
        if(_poseActiveVisuals[poseNumber] != null)
        _poseActiveVisuals[poseNumber].gameObject.SetActive(false);
    }
}
