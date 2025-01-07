# Saga Orchestration with MassTransit

This project demonstrates the implementation of the Saga Orchestration pattern using MassTransit in a .NET 8 application. The project consists of three APIs: Stock, Order, and Payment, which are coordinated using a state machine to handle events and manage the workflow.

## Project Structure

- **SagaStateMachine.Service**: Contains the state machine logic and state instances.
- **Shared**: Contains shared messages, events, and settings used across the services.

## State Machine

The state machine is defined in the `OrderStateMachine` class, which extends `MassTransitStateMachine<OrderStateInstance>`. It handles various events and transitions between different states.

### Events

- `OrderStartedEvent`
- `StockReservedEvent`
- `PaymentCompletedEvent`
- `StockNotReservedEvent`
- `PaymentFailedEvent`

### States

- `OrderCreated`
- `StockReserved`
- `StockNotReserved`
- `PaymentCompleted`
- `PaymentFailed`

### State Transitions

1. **OrderStartedEvent**: 
   - Initializes the state machine and transitions to `OrderCreated`.
   - Sends `OrderCreatedEvent` to the Stock service.

2. **StockReservedEvent**:
   - Transitions to `StockReserved`.
   - Sends `PaymentStartedEvent` to the Payment service.

3. **StockNotReservedEvent**:
   - Transitions to `StockNotReserved`.
   - Sends `OrderFailedEvent` to the Order service.

4. **PaymentCompletedEvent**:
   - Transitions to `PaymentCompleted`.
   - Sends `OrderCompletedEvent` to the Order service.
   - Finalizes the state machine.

5. **PaymentFailedEvent**:
   - Transitions to `PaymentFailed`.
   - Sends `OrderFailedEvent` to the Order service.
   - Sends `StockRollbackMessage` to the Stock service.

## Usage

### Prerequisites

- .NET 8 SDK
- RabbitMQ (for message brokering)

### Running the Application

1. Clone the repository.
2. Navigate to the project directory.
3. Restore the dependencies:
   
