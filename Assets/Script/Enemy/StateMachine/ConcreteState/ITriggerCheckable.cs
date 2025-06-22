using UnityEngine;
public interface ITriggerCheckable
{
    bool isAggroed { get; set; }
    bool isInAttackRange { get; set; }

    void setAggroStatus(bool isAggroed);
    void setAttackRangeStatus(bool isInAttackRange);
}