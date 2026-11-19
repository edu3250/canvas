using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public class NavigationTests
{
    private static readonly SpaceId A = new("Programación"), B = new("Juegos");
    private static readonly Camera Home = new(new Point(0,0),1), First = new(new Point(120,80),0.75),
        Second = new(new Point(60,40),1), Pan = new(new Point(25,10),1);
    private static SessionState Start() => new(A, new Dictionary<SpaceId, SpaceNavigation>
    {
        [A] = new(First, Home, null), [B] = new(Second, Home, null)
    });
    private static SessionState Step(SessionState s, NavigationCommand cmd)
    {
        var result = Navigation.Apply(s, cmd);
        Assert.Null(result.Error);
        return result.State;
    }
    private static void Check(SessionState s, SpaceId active, Camera a, Camera? ar, Camera b, Camera? br)
    {
        Assert.Equal(active, s.ActiveSpaceId);
        Assert.Equal(2, s.Spaces.Count);
        Assert.Equal(new SpaceNavigation(a, Home, ar), s.Spaces[A]);
        Assert.Equal(new SpaceNavigation(b, Home, br), s.Spaces[B]);
    }
    [Fact]
    public void KeepsSeparateViewsAndRestoresEachReturn()
    {
        var original = Start();
        var s = Step(original, new ToggleHome());
        Check(s,A,Home,First,Second,null);
        Check(Step(s,new SwitchSpace(A)),A,Home,First,Second,null);
        s = Step(s,new SetCamera(Pan));
        Check(s,A,Pan,First,Second,null);
        Check(Step(s,new ToggleHome()),A,First,null,Second,null);
        s = Step(original,new ToggleHome());
        s = Step(s,new SwitchSpace(B));
        Check(s,B,Home,First,Second,null);
        s = Step(Step(s,new ToggleHome()),new SetCamera(Pan));
        Check(s,B,Home,First,Pan,Second);
        s = Step(s,new SwitchSpace(A));
        Check(s,A,Home,First,Pan,Second);
        Check(Step(s,new ToggleHome()),A,First,null,Pan,Second);
        Check(original,A,First,null,Second,null);
    }
    [Fact]
    public void InvalidCommandsPreserveTheEntireState()
    {
        var s = Step(Start(),new ToggleHome());
        void Reject(NavigationCommand cmd, NavigationError error)
        {
            var r = Navigation.Apply(s,cmd);
            Assert.Equal<NavigationError?>(error,r.Error);
            Check(s,A,Home,First,Second,null);
            Check(r.State,A,Home,First,Second,null);
        }
        Reject(new SwitchSpace(new("Otro")),NavigationError.UnknownSpace);
        foreach (var n in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        foreach (var p in new[] { new Point(n,0), new Point(0,n) })
            Reject(new SetCamera(new(p,1)),NavigationError.InvalidGeometry);
        foreach (var z in new[] { 0d,-1,double.NaN,double.PositiveInfinity,double.NegativeInfinity })
            Reject(new SetCamera(new(new Point(0,0),z)),NavigationError.InvalidZoom);
    }
}
