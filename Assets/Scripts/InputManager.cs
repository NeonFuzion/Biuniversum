using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MovementInput(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Vector2 movementInput = context.ReadValue<Vector2>();
        Movement(movementInput);
    }

    public void Movement(Vector2 movementInput)
    {
        BattleManager.Instance.Movement(movementInput);
    }

    public void BasicAttackInput(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        BasicAttack();
    }

    public void BasicAttack()
    {
        BattleManager.Instance.Action(0);
    }

    public void Skill1Input(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Skill1();
    }

    public void Skill1()
    {
        BattleManager.Instance.Action(1);
    }

    public void EndTurnInput(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        EndTurn();
    }

    public void EndTurn()
    {
        BattleManager.Instance.EndTurn();
    }

    public void SelectNextCharacterInput(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        SelectNextCharacter();
    }

    public void SelectCharacter1Input(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        SelectNextCharacter(0);
    }

    public void SelectCharacter2Input(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        SelectNextCharacter(1);
    }

    public void SelectCharacter3Input(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        SelectNextCharacter(2);
    }

    public void SelectCharacter4Input(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        SelectNextCharacter(3);
    }

    public void SelectNextCharacter(int index = -1)
    {
        BattleManager.Instance.ChangeEntity(index);
    }
}
