namespace Companion.Signaling.Core.Test;

public class DerivedSignalTest
{
    [Fact]
    public void TestGet()
    {
        // arrange
        var recalculationCount = 0;

        var signal = new Signal<int>(1);

        var derivedSignal = new DerivedSignal<int>(() =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        var value = derivedSignal.Get();

        // assert
        Assert.Equal(1, value);
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestValueAccessedMultipleTimes()
    {
        // arrange
        var recalculationCount = 0;

        var signal = new Signal<int>(1);

        var derivedSignal = new DerivedSignal<int>(() =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        var value1 = derivedSignal.Get();
        var value2 = derivedSignal.Get();

        // assert
        Assert.Equal(1, value1);
        Assert.Equal(1, value2);
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestValueChanged()
    {
        // arrange
        var recalculationCount = 0;

        var signal = new Signal<int>(1);

        var derivedSignal = new DerivedSignal<int>(() =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        signal.Set(2);

        // assert
        Assert.Equal(2, derivedSignal.Get());
        Assert.Equal(2, recalculationCount);
    }

    [Fact]
    public void TestValueNotChanged()
    {
        // arrange
        var recalculationCount = 0;

        var signal = new Signal<int>(1);

        var derivedSignal = new DerivedSignal<int>(() =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        signal.Set(1);

        // assert
        Assert.Equal(1, derivedSignal.Get());
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestValueChangedMultipleTimes()
    {
        // arrange
        var recalculationCount = 0;

        var signal = new Signal<int>(1);

        var derivedSignal = new DerivedSignal<int>(() =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        signal.Set(2);
        signal.Set(3);

        // assert
        Assert.Equal(3, derivedSignal.Get());
        Assert.Equal(3, recalculationCount);
    }

    [Fact]
    public void TestUnreferencedValueChanged()
    {
        // arrange
        var recalculationCount = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var derivedSignal = new DerivedSignal<int>(() =>
        {
            recalculationCount++;
            return signal1.Get();
        });

        // act
        signal2.Set(3);

        // assert
        Assert.Equal(1, derivedSignal.Get());
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestUnreferencedValueChanged_WasReferenced()
    {
        // arrange
        var recalculationCount = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var derivedSignal = new DerivedSignal<int>(() =>
        {
            recalculationCount++;
            if (signal1.Get() == 3)
                return signal1.Get();
            return signal2.Get();
        });

        // act
        signal1.Set(3);
        signal2.Set(4);

        // assert
        Assert.Equal(3, derivedSignal.Get());
        Assert.Equal(2, recalculationCount);
    }

    [Fact]
    public void TestNestedDerivedSignalValue()
    {
        // arrange
        var recalculationCount1 = 0;
        var recalculationCount2 = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var derivedSignal1 = new DerivedSignal<int>(() =>
        {
            recalculationCount1++;
            return signal1.Get();
        });

        var derivedSignal2 = new DerivedSignal<int>(() =>
        {
            recalculationCount2++;
            return derivedSignal1.Get() + signal2.Get();
        });

        // act
        var value = derivedSignal2.Get();

        // assert
        Assert.Equal(3, value);
        Assert.Equal(1, recalculationCount1);
        Assert.Equal(1, recalculationCount2);
    }

    [Fact]
    public void TestNestedDerivedSignalValue_ValueChanged()
    {
        // arrange
        var recalculationCount1 = 0;
        var recalculationCount2 = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var derivedSignal1 = new DerivedSignal<int>(() =>
        {
            recalculationCount1++;
            return signal1.Get();
        });

        var derivedSignal2 = new DerivedSignal<int>(() =>
        {
            recalculationCount2++;
            return derivedSignal1.Get() + signal2.Get();
        });

        // act
        signal2.Set(3);

        // assert
        Assert.Equal(4, derivedSignal2.Get());
        Assert.Equal(1, recalculationCount1);
        Assert.Equal(2, recalculationCount2);
    }

    [Fact]
    public void TestNestedDerivedSignalValue_NestedValueChanged()
    {
        // arrange
        var recalculationCount1 = 0;
        var recalculationCount2 = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var derivedSignal1 = new DerivedSignal<int>(() =>
        {
            recalculationCount1++;
            return signal1.Get();
        });

        var derivedSignal2 = new DerivedSignal<int>(() =>
        {
            recalculationCount2++;
            return derivedSignal1.Get() + signal2.Get();
        });

        // act
        signal1.Set(3);

        // assert
        Assert.Equal(5, derivedSignal2.Get());
        Assert.Equal(2, recalculationCount1);
        Assert.Equal(2, recalculationCount2);
    }

    [Fact]
    public async Task TestMultipleSignalChangesAtOnce()
    {
        // arrange
        var recalculationCount = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var signal3 = new Signal<int>(3);

        var taskCompletionSource1 = new TaskCompletionSource();
        var taskCompletionSource2 = new TaskCompletionSource();

        taskCompletionSource1.SetResult();
        var derivedSignal = new DerivedSignal<int>(() =>
        {
            signal1.Get();
            signal2.Get();
            signal3.Get();

            taskCompletionSource2.SetResult();
            taskCompletionSource1.Task.Wait();

            recalculationCount++;
            return signal3.Get();
        });

        // act
        taskCompletionSource1 = new();
        taskCompletionSource2 = new();

        var setterTask1 = Task.Run(() => signal1.Set(4));
        await taskCompletionSource2.Task;

        var setterTask2 = Task.Run(() => signal2.Set(5));
        while (!derivedSignal!.IsEvaluationQueued)
            ;

        signal3.Set(6);

        taskCompletionSource2 = new();
        taskCompletionSource1.SetResult();

        await setterTask1;
        await setterTask2;

        // assert
        Assert.Equal(6, derivedSignal.Get());
        Assert.Equal(3, recalculationCount);
    }

    [Fact]
    public void TestExceptionDuringComputation()
    {
        // arrange
        var recalculationCount = 0;
        var signal = new Signal<int>(0);

        // act & assert
        Assert.Throws<InvalidOperationException>(() => new DerivedSignal<int>(
            () =>
            {
                signal.Get();

                recalculationCount++;
                throw new InvalidOperationException();
            }));

        Assert.Equal(1, recalculationCount);

        signal.Set(1);
        Assert.Equal(1, recalculationCount);
    }
}
