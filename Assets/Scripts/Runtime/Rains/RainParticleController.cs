using UnityEngine;

namespace Monos.WSIM.Runtime.Rains
{
    public class RainParticleController : MonoBehaviour
    {
        [SerializeField] ParticleSystem[] particles;

        public bool Active { get; private set; }

        public void ToggleActive(bool active)
        {
            Active = active;

            for (int i = 0; i < particles.Length; i++)
            {
                if (active)
                {
                    this.gameObject.layer = LayerMask.NameToLayer("Rain");
                    //particles[i].Simulate(0, true, true);
                    particles[i].Play(true);
                }
                else
                {
                    this.gameObject.layer = LayerMask.NameToLayer("Not Rendered");
                    particles[i].Stop(true);
                }
            }
        }
    }
}