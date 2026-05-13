using UnityEngine;
using UnityEngine.SceneManagement;

public class Lazur : MonoBehaviour
{
    Vector3 d = Vector3.zero;
    float s = 0;


    void Start()
    {
        
    }
    void Update()
    {
        transform.position += d * s * Time.deltaTime;
    }


    public void Launch(Vector3 direction, float speed)
    {
        d = direction;
        s = speed;
    }
    
    private void OnTriggerEnter(Collider other) 
    {
        if (other.tag == "Player")
            SceneManager.LoadScene(0);
    }
}
