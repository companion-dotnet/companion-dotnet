namespace Companion.Signaling.Core.Test;

public class SignalingContextTest
{
    [Fact]
    public void TestContextDisposed()
    {
        // arrange
        var signalingContext = new SignalingContext();
        
        // act
        signalingContext.Dispose();

        // assert
        Assert.Throws<InvalidOperationException>(
            () => new Signal<int>(signalingContext, 0));
        Assert.Throws<InvalidOperationException>(
            () => new DerivedSignal<int>(signalingContext, () => 0));
        Assert.Throws<InvalidOperationException>(
            () => new SignalingEffect(signalingContext, () => { }));
    }

    [Fact]
    public void TestSignal()
    {
        // arrange
        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(signalingContext, 1);

        // act
        signal.Set(2);

        // assert
        Assert.Equal(2, signal.Get());
    }

    [Fact]
    public void TestSignal_ContextDisposed()
    {
        // arrange
        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(signalingContext, 1);

        // act
        signalingContext.Dispose();

        // assert
        Assert.Throws<InvalidOperationException>(
            () => signal.Get());
        Assert.Throws<InvalidOperationException>(
            () => signal.Set(2));
    }

    [Fact]
    public void TestSignalInUseByDerivedSignal_ContextDisposed()
    {
        // arrange
        var recalculationCount = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(signalingContext, 1);
        var derivedSignal = new DerivedSignal<int>(() =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        signalingContext.Dispose();

        // assert
        Assert.Throws<InvalidOperationException>(
            () => signal.Set(2));

        Assert.Equal(1, derivedSignal.Get());
        Assert.Equal(1, recalculationCount);
    }
    
    [Fact]
    public void TestSignalInUseBySignalingEffect_ContextDisposed()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(signalingContext, 1);
        _ = new SignalingEffect(() =>
        {
            recalculationCount++;
            value = signal.Get();
        });

        // act
        signalingContext.Dispose();

        // assert
        Assert.Throws<InvalidOperationException>(
            () => signal.Set(2));

        Assert.Equal(1, value);
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestDerivedSignal()
    {
        // arrange
        var recalculationCount = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(1);
        var derivedSignal = new DerivedSignal<int>(signalingContext, () =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        signal.Set(2);

        // assert
        Assert.Equal(2, recalculationCount);
        Assert.Equal(2, derivedSignal.Get());
    }

    [Fact]
    public void TestDerivedSignal_ContextDisposed()
    {
        // arrange
        var recalculationCount = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(1);
        var derivedSignal = new DerivedSignal<int>(signalingContext, () =>
        {
            recalculationCount++;
            return signal.Get();
        });

        // act
        signalingContext.Dispose();
        signal.Set(2);

        // assert
        Assert.Equal(1, recalculationCount);
        Assert.Throws<InvalidOperationException>(
            () => derivedSignal.Get());
    }
    
    [Fact]
    public void TestDerivedSignalInUseByDerivedSignal_ContextDisposed()
    {
        // arrange
        var recalculationCount = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(1);
        var derivedSignal1 = new DerivedSignal<int>(signalingContext, signal.Get);
        var derivedSignal2 = new DerivedSignal<int>(() =>
        {
            recalculationCount++;
            return derivedSignal1.Get();
        });

        // act
        signalingContext.Dispose();
        signal.Set(2);

        // assert
        Assert.Equal(1, recalculationCount);
        Assert.Equal(1, derivedSignal2.Get());
    }
    
    [Fact]
    public void TestDerivedSignalInUseBySignalingEffect_ContextDisposed()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(1);
        var derivedSignal = new DerivedSignal<int>(signalingContext, signal.Get);
        var signalingEffect = new SignalingEffect(signalingContext, () =>
        {
            recalculationCount++;
            value = derivedSignal.Get();
        });

        // act
        signalingContext.Dispose();
        signal.Set(2);

        // assert
        Assert.Equal(1, value);
        Assert.Equal(1, recalculationCount);
    }

    [Fact]
    public void TestSignalingEffect()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(1);
        var signalingEffect = new SignalingEffect(signalingContext, () =>
        {
            recalculationCount++;
            value = signal.Get();
        });

        // act
        signal.Set(2);

        // assert
        Assert.Equal(2, recalculationCount);
        Assert.Equal(2, value);
    }

    [Fact]
    public void TestSignalingEffect_ContextDisposed()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signalingContext = new SignalingContext();
        var signal = new Signal<int>(1);
        var signalingEffect = new SignalingEffect(signalingContext , () =>
        {
            recalculationCount++;
            value = signal.Get();
        });

        // act
        signalingContext.Dispose();
        signal.Set(2);

        // assert
        Assert.Equal(1, recalculationCount);
        Assert.Equal(1, value);
    }
}
