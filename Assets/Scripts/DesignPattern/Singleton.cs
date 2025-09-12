using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {

	private static T _instance;

	public static T Instance
	{
		get
		{
			if(Singleton<T>._instance == null)
			{
				Singleton<T>._instance = (T) Object.FindObjectOfType(typeof(T));
				if(Singleton<T>._instance == null)
					Singleton<T>._instance = (T) new GameObject("_SingleBehaviour_<" + typeof (T).ToString() + ">").AddComponent<T>();
			}
			return Singleton<T>._instance;
		}
	}
}

public class BSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                return _instance ?? (_instance = (T)FindObjectOfType(typeof(T)));
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
    }
}