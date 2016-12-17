using UnityEngine;

public class BulletScript : MonoBehaviour
{
    #region Unity
    private void OnCollisionEnter(Collision col)
    {
        Mewzor.InputManager im = col.gameObject.GetComponent<Mewzor.InputManager>();
        if (im)
        {
            im.Health -= 1;
            Destroy(this.gameObject);
            this.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (this.transform.position.y < -10f)
        {
            Destroy(this.gameObject);
            this.enabled = false;
        }
	}
    #endregion
}
