using UnityEngine;

namespace ArtboxGames
{
    public class RotationScript : MonoBehaviour
    {
        public float speed = 30f;

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(new Vector3(0, 0, Time.deltaTime * speed));
        }
    }
}