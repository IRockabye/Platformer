using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCarpet : MonoBehaviour
{
    public Transform left, right;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            RaycastHit2D leftWall = Physics2D.Raycast(left.position, Vector2.left, 0.1f);
            RaycastHit2D righttWall = Physics2D.Raycast(right.position, Vector2.right, 0.1f);

            if (((Input.GetAxis("Horizontal") > 0) && !righttWall.collider && (collision.transform.position.x > transform.position.x)) || ((Input.GetAxis("Horizontal") < 0) && !leftWall.collider && (collision.transform.position.x < transform.position.x)))
            {
                transform.position = new Vector3(collision.transform.position.x, transform.position.y, collision.transform.position.z); 
            }
                
        }    
    }
}
