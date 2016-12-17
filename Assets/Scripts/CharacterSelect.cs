using UnityEngine;

public class CharacterSelect : MonoBehaviour
{
    private int character = 1;

    #region Unity
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.ChangeCharacter(-1);
        }

        if (Input.GetButtonDown("Jump"))
        {
            this.SelectCharacter(this.character);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.ChangeCharacter(1);
        }

        Vector3 position = this.transform.position;
        position.z = Mathf.Lerp(position.z, 4 + ((this.character+1)*4), Time.deltaTime * 5f);
        this.transform.position = position;

        Camera.main.transform.position = this.transform.position;
    }
    #endregion
    
    private void ChangeCharacter(int mod)
    {
        this.character += mod;

        if (this.character > 2)
        {
            this.character = 2;
        }

        if (this.character < 0)
        {
            this.character = 0;
        }
    }

    private void SelectCharacter(int num)
    {
        // TODO: Actually select the right character
        // HACK: Always selects character one.

        if (num != 2)
        {
            Debug.LogWarning("We only implemented the first character so far!");
            return;
        }

        GameObject go = GameObject.Find("Query-Chan_Mechanim");
        go.GetComponentInChildren<FlipCamera>().enabled = true;
        go.GetComponentInChildren<FlipCamera>().UpdateCamera = false;

        go.GetComponentInChildren<UseWiimote>().enabled = true;
        go.GetComponentInChildren<UseWiimote>().UpdateCamera = true;

        go.GetComponentInChildren<AnimatedPhysics>().enabled = true;
    }
}
