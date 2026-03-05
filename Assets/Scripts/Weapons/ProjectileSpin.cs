using UnityEngine;

namespace SurvivorSeries.Weapons
{
    public class ProjectileSpin : MonoBehaviour
    {
        [SerializeField] private Vector3 _axis = Vector3.right;
        [SerializeField] private float _degreesPerSecond = 720f;

        private void Update()
        {
            transform.Rotate(_axis, _degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
