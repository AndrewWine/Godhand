using UnityEngine;

public class SingletonDontDestroy<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if(SingletonDontDestroy<T>._instance == null)
            {
                SingletonDontDestroy<T>._instance = (T) Object.FindObjectOfType(typeof(T));
                if(SingletonDontDestroy<T>._instance == null)
                    SingletonDontDestroy<T>._instance = (T) new GameObject("_SingleBehaviour_<" + typeof (T).ToString() + ">").AddComponent<T>();
				
                DontDestroyOnLoad(_instance.gameObject);
            }
            return SingletonDontDestroy<T>._instance;
        }
    }
	
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // Hủy object nếu có một instance khác đã tồn tại
        }
    }
}