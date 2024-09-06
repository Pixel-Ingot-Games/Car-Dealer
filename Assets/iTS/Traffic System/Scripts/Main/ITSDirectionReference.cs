using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ITSDirectionReference : MonoBehaviour {
	

	
void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(transform.position + transform.forward , transform.position + transform.forward + transform.right * 2);
		Gizmos.DrawLine(transform.position + transform.forward * 2 , transform.position + transform.forward * 2 + transform.right * 2);
		Gizmos.DrawLine(transform.position + transform.forward * 3 ,transform.position + transform.forward * 3 + transform.right * 2);
		Gizmos.DrawLine(transform.position + transform.forward * 4 ,transform.position + transform.forward * 4 + transform.right * 2);
		Gizmos.DrawLine(transform.position + transform.forward * 5 ,transform.position + transform.forward * 5 + transform.right * 2);
		Gizmos.DrawLine(transform.position + transform.forward * 6 ,transform.position + transform.forward * 6 + transform.right * 2);
		Gizmos.DrawLine(transform.position - transform.forward ,transform.position - transform.forward + transform.right * 2);
		Gizmos.DrawLine(transform.position - transform.forward * 2 ,transform.position - transform.forward * 2 + transform.right * 2);
		Gizmos.DrawLine(transform.position - transform.forward * 3 , transform.position - transform.forward * 3 + transform.right * 2);
		Gizmos.DrawLine(transform.position - transform.forward * 4 , transform.position - transform.forward * 4 + transform.right * 2);
		Gizmos.DrawLine(transform.position - transform.forward * 5 , transform.position - transform.forward * 5 + transform.right * 2);
		Gizmos.DrawLine(transform.position - transform.forward * 6 , transform.position - transform.forward * 6 + transform.right * 2);
		
	
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position - transform.forward * 2 , transform.forward * 2);
		Gizmos.DrawLine(transform.forward * 2, -transform.up * 2 + transform.forward * 2);
		Gizmos.DrawLine(-transform.up * 2 + transform.forward * 2, transform.forward * 5f + transform.up);
		Gizmos.DrawLine(transform.forward * 5f + transform.up , transform.up * 4 + transform.forward * 2);
		Gizmos.DrawLine(transform.up * 4 + transform.forward * 2, transform.up * 2 + transform.forward * 2);
		Gizmos.DrawLine(transform.up * 2 + transform.forward * 2,transform.position+ transform.up * 2- transform.forward * 2);
		Gizmos.DrawLine(transform.position+ transform.up * 2- transform.forward * 2, transform.position - transform.forward * 2);
		
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.right * 15);
		Gizmos.DrawLine(transform.position, -transform.right * 15);
		
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, transform.up * 15);
		Gizmos.DrawLine(transform.position, -transform.up * 15);
		
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, transform.forward * 15);
		Gizmos.DrawLine(transform.position, -transform.forward * 15);
		
	}	
}
