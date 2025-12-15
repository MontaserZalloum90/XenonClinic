namespace XenonClinic.WorkflowEngine.Tests.StateMachine;

using FluentAssertions;
using XenonClinic.WorkflowEngine.Core.StateMachine;
using Xunit;

public class StateMachineTests
{
    public enum OrderState
    {
        Created,
        Pending,
        Approved,
        Rejected,
        Fulfilled,
        Cancelled
    }

    public enum OrderTrigger
    {
        Submit,
        Approve,
        Reject,
        Fulfill,
        Cancel
    }

    [Fact]
    public void Builder_CreatesStateMachineWithStates()
    {
        var stateMachine = StateMachineBuilder<OrderState>.Create()
            .DefineState(OrderState.Created)
                .WithEntry(ctx => { /* log created */ })
                .WithTransition(OrderTrigger.Submit, OrderState.Pending)
                .FinishState()
            .DefineState(OrderState.Pending)
                .WithTransition(OrderTrigger.Approve, OrderState.Approved)
                .WithTransition(OrderTrigger.Reject, OrderState.Rejected)
                .FinishState()
            .DefineState(OrderState.Approved)
                .AsEndState()
                .FinishState()
            .DefineState(OrderState.Rejected)
                .AsEndState()
                .FinishState()
            .WithInitialState(OrderState.Created)
            .Build();

        stateMachine.Should().NotBeNull();
        stateMachine.States.Should().HaveCount(4);
        stateMachine.InitialState.Should().Be(OrderState.Created);
    }

    [Fact]
    public void GetAvailableTransitions_ReturnsCorrectTransitions()
    {
        var stateMachine = StateMachineBuilder<OrderState>.Create()
            .DefineState(OrderState.Created)
                .WithTransition(OrderTrigger.Submit, OrderState.Pending)
                .WithTransition(OrderTrigger.Cancel, OrderState.Cancelled)
                .FinishState()
            .DefineState(OrderState.Pending)
                .WithTransition(OrderTrigger.Approve, OrderState.Approved)
                .FinishState()
            .DefineState(OrderState.Approved).AsEndState().FinishState()
            .DefineState(OrderState.Cancelled).AsEndState().FinishState()
            .WithInitialState(OrderState.Created)
            .Build();

        var transitions = stateMachine.GetAvailableTransitions(OrderState.Created);

        transitions.Should().HaveCount(2);
        transitions.Should().Contain(t => t.Trigger.Equals(OrderTrigger.Submit));
        transitions.Should().Contain(t => t.Trigger.Equals(OrderTrigger.Cancel));
    }

    [Fact]
    public void GetState_ReturnsCorrectStateDefinition()
    {
        var stateMachine = StateMachineBuilder<OrderState>.Create()
            .DefineState(OrderState.Created)
                .WithTransition(OrderTrigger.Submit, OrderState.Pending)
                .FinishState()
            .DefineState(OrderState.Pending).FinishState()
            .WithInitialState(OrderState.Created)
            .Build();

        var state = stateMachine.GetState(OrderState.Created);

        state.Should().NotBeNull();
        state.StateId.Should().Be(OrderState.Created);
    }

    [Fact]
    public void IsEndState_ReturnsCorrectValue()
    {
        var stateMachine = StateMachineBuilder<OrderState>.Create()
            .DefineState(OrderState.Created)
                .WithTransition(OrderTrigger.Submit, OrderState.Pending)
                .FinishState()
            .DefineState(OrderState.Pending).FinishState()
            .DefineState(OrderState.Approved)
                .AsEndState()
                .FinishState()
            .WithInitialState(OrderState.Created)
            .Build();

        var createdState = stateMachine.GetState(OrderState.Created);
        var approvedState = stateMachine.GetState(OrderState.Approved);

        createdState.IsEndState.Should().BeFalse();
        approvedState.IsEndState.Should().BeTrue();
    }
}

public class StateMachineExecutorTests
{
    public enum TrafficLightState { Red, Yellow, Green }
    public enum TrafficLightTrigger { Timer, Emergency }

    private readonly StateMachine<TrafficLightState> _stateMachine;
    private readonly StateMachineExecutor<TrafficLightState> _executor;

    public StateMachineExecutorTests()
    {
        _stateMachine = StateMachineBuilder<TrafficLightState>.Create()
            .DefineState(TrafficLightState.Red)
                .WithTransition(TrafficLightTrigger.Timer, TrafficLightState.Green)
                .WithTransition(TrafficLightTrigger.Emergency, TrafficLightState.Red)
                .FinishState()
            .DefineState(TrafficLightState.Green)
                .WithTransition(TrafficLightTrigger.Timer, TrafficLightState.Yellow)
                .WithTransition(TrafficLightTrigger.Emergency, TrafficLightState.Red)
                .FinishState()
            .DefineState(TrafficLightState.Yellow)
                .WithTransition(TrafficLightTrigger.Timer, TrafficLightState.Red)
                .WithTransition(TrafficLightTrigger.Emergency, TrafficLightState.Red)
                .FinishState()
            .WithInitialState(TrafficLightState.Red)
            .Build();

        _executor = new StateMachineExecutor<TrafficLightState>(_stateMachine);
    }

    [Fact]
    public async Task FireAsync_ValidTransition_ReturnsSuccessfulResult()
    {
        var result = await _executor.FireAsync(TrafficLightState.Red, TrafficLightTrigger.Timer, null);

        result.IsSuccess.Should().BeTrue();
        result.NewState.Should().Be(TrafficLightState.Green);
    }

    [Fact]
    public async Task FireAsync_InvalidTransition_ReturnsFailure()
    {
        // Create a state machine without a transition from Yellow to Green
        var stateMachine = StateMachineBuilder<TrafficLightState>.Create()
            .DefineState(TrafficLightState.Yellow)
                .WithTransition(TrafficLightTrigger.Timer, TrafficLightState.Red)
                .FinishState()
            .DefineState(TrafficLightState.Red).FinishState()
            .WithInitialState(TrafficLightState.Yellow)
            .Build();

        var executor = new StateMachineExecutor<TrafficLightState>(stateMachine);

        // Try an invalid trigger (Emergency not defined for Yellow in this limited machine)
        var result = await executor.FireAsync(TrafficLightState.Yellow, TrafficLightTrigger.Emergency, null);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void GetAvailableTransitions_ReturnsAllPossibleTransitions()
    {
        var transitions = _executor.GetAvailableTransitions(TrafficLightState.Red, null);

        transitions.Should().HaveCount(2);
    }

    [Fact]
    public async Task FireAsync_EmergencyFromAnyState_GoesToRed()
    {
        // From Green
        var result = await _executor.FireAsync(TrafficLightState.Green, TrafficLightTrigger.Emergency, null);
        result.IsSuccess.Should().BeTrue();
        result.NewState.Should().Be(TrafficLightState.Red);

        // From Yellow
        result = await _executor.FireAsync(TrafficLightState.Yellow, TrafficLightTrigger.Emergency, null);
        result.IsSuccess.Should().BeTrue();
        result.NewState.Should().Be(TrafficLightState.Red);
    }
}

public class StateMachineWithGuardsTests
{
    public enum DocumentState { Draft, Review, Approved, Published }
    public enum DocumentTrigger { Submit, Approve, Publish, Reject }

    [Fact]
    public async Task FireAsync_WithPassingGuard_TransitionsSuccessfully()
    {
        var stateMachine = StateMachineBuilder<DocumentState>.Create()
            .DefineState(DocumentState.Review)
                .WithTransition(DocumentTrigger.Approve, DocumentState.Approved)
                    .WithGuard((ctx) => Task.FromResult(true)) // Guard passes
                .WithTransition(DocumentTrigger.Reject, DocumentState.Draft)
                .FinishState()
            .DefineState(DocumentState.Approved).FinishState()
            .DefineState(DocumentState.Draft).FinishState()
            .WithInitialState(DocumentState.Review)
            .Build();

        var executor = new StateMachineExecutor<DocumentState>(stateMachine);
        var result = await executor.FireAsync(DocumentState.Review, DocumentTrigger.Approve, null);

        result.IsSuccess.Should().BeTrue();
        result.NewState.Should().Be(DocumentState.Approved);
    }

    [Fact]
    public async Task FireAsync_WithFailingGuard_DoesNotTransition()
    {
        var stateMachine = StateMachineBuilder<DocumentState>.Create()
            .DefineState(DocumentState.Review)
                .WithTransition(DocumentTrigger.Approve, DocumentState.Approved)
                    .WithGuard((ctx) => Task.FromResult(false)) // Guard fails
                .FinishState()
            .DefineState(DocumentState.Approved).FinishState()
            .WithInitialState(DocumentState.Review)
            .Build();

        var executor = new StateMachineExecutor<DocumentState>(stateMachine);
        var result = await executor.FireAsync(DocumentState.Review, DocumentTrigger.Approve, null);

        result.IsSuccess.Should().BeFalse();
    }
}
