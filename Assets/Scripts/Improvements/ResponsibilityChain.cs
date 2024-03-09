using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  DESCRIPTION: 
 *    - A ResponsibilityLink conditionaly changes outState based on the value of inState.
 *  PARAMETERS: 
 *    - inState - used to determine the actions taken by this link
 *    - outState - object that actions are applied to
 *  RETURN:
 *    - a boolean indicating whether a chain should continue executing subsequent links
 */
public interface ResponsibilityLink<I, O> {
  public bool execute(I inState, O outState);
}
