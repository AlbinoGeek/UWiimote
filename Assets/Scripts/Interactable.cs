// <copyright file="Interactable.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using UnityEngine;

/// <summary>
/// 
/// </summary>
[DisallowMultipleComponent]
[ExecuteInEditMode]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class Interactable : MonoBehaviour
{
    public Vector3 GrabOffset = Vector3.zero;

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(this.transform.position + GrabOffset, .04f);
    }
}
