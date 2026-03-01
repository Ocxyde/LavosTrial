using UnityEngine;

namespace Code.Lavos.Core
{
    public class PersistentUI : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}