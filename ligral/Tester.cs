using System.Collections.Generic;
using Dict=System.Collections.Generic.Dictionary<string,object>;
using Ligral.Models;

namespace Ligral
{
    class Tester
    {
        public void Test()
        {
            TestInspector();
        }
        private void TestScope()
        {
            SineWave sine = new SineWave();
            Scope scope = new Scope();
            sine.Connect(scope);
            for (int i=1;i<100;i++)
            {
                double time = i/10.0;
                sine.Update(time);
                scope.Update(time);
                sine.Propagate();
                scope.Propagate();
            }
            scope.Release();
        }
        private void TestPhaseDiagram()
        {
            SineWave sine1 = new SineWave();
            SineWave sine2 = new SineWave();
            Dict conf = new Dict();
            conf.Add("omega", 2);
            sine2.Configure(conf);
            PhaseDiagram pd = new PhaseDiagram();
            sine1.Connect(0, pd.Expose(0));
            sine2.Connect(0, pd.Expose(1));
            for (int i=1;i<100;i++)
            {
                double time = i/10.0;
                sine1.Update(time);
                sine2.Update(time);
                pd.Update(time);
                sine1.Propagate();
                sine2.Propagate();
                pd.Propagate();
            }
            pd.Release();
        }
        private void TestInspector()
        {
            double k = 5;
            double b = 1;
            double m = 5;
            double Fu_ampl = 10;
            double x10 = 0;
            double x20 = 0;
            double start = 2;

            Dict conf = new Dict();
            List<Model> nodes = new List<Model>();

            Node xddot = new Node();
            xddot.Name = "xddot";
            nodes.Add(xddot);

            Integrator integrator1 = new Integrator();
            nodes.Add(integrator1);
            conf["initial"] = x10;
            integrator1.Configure(conf);
            xddot.Connect(integrator1);

            Node xdot = new Node();
            nodes.Add(xdot);
            xdot.Name = "xdot";
            integrator1.Connect(xdot);

            Scope xdot_sc = new Scope();
            nodes.Add(xdot_sc);
            xdot_sc.Name = "xdot_sc";
            xdot.Connect(xdot_sc);

            Integrator integrator2 = new Integrator();
            nodes.Add(integrator2);
            conf["initial"] = x20;
            xdot.Connect(integrator2);

            Node x = new Node();
            nodes.Add(x);
            x.Name = "x";
            integrator2.Connect(x);

            Scope x_sc = new Scope();
            nodes.Add(x_sc);
            x_sc.Name = "x_sc";
            x.Connect(x_sc);

            Abs absolute1 = new Abs();
            nodes.Add(absolute1);
            xdot.Connect(absolute1);

            Node absxdot = new Node();
            nodes.Add(absxdot);
            absxdot.Name = "absxdot";
            absolute1.Connect(absxdot);

            Calculator calculator1 = new Calculator();
            nodes.Add(calculator1);
            conf["type"] = "*";
            calculator1.Configure(conf);
            xdot.Connect(0, calculator1.Expose(0));
            absxdot.Connect(0, calculator1.Expose(1));

            Gain gain1 = new Gain();
            nodes.Add(gain1);
            conf["value"] = b;
            calculator1.Connect(gain1);

            Node Fb = new Node();
            nodes.Add(Fb);
            Fb.Name = "Fb";
            gain1.Connect(Fb);

            Gain gain2 = new Gain();
            nodes.Add(gain2);
            conf["value"] = k;
            x.Connect(gain2);

            Node Fk = new Node();
            nodes.Add(Fk);
            Fk.Name = "Fk";
            gain2.Connect(Fk);

            Scope Fk_sc = new Scope();
            nodes.Add(Fk_sc);
            Fk_sc.Name = "Fk_sc";
            Fk.Connect(Fk_sc);

            Step step1 = new Step();
            nodes.Add(step1);
            conf["start"] = start;
            conf["level"] = Fu_ampl;
            step1.Configure(conf);

            Node Fu = new Node();
            nodes.Add(Fu);
            Fu.Name = "Fu";
            step1.Connect(Fu);

            Scope Fu_sc = new Scope();
            nodes.Add(Fu_sc);
            Fu_sc.Name = "Fu_sc";
            Fu.Connect(Fu_sc);
            
            Calculator calculator2 = new Calculator();
            nodes.Add(calculator2);
            conf["type"] = "+";
            calculator2.Configure(conf);
            Fk.Connect(0, calculator2.Expose(0));
            Fb.Connect(0, calculator2.Expose(1));
            
            Calculator calculator3 = new Calculator();
            nodes.Add(calculator3);
            conf["type"] = "-";
            calculator3.Configure(conf);
            Fu.Connect(0, calculator3.Expose(0));
            calculator2.Connect(0, calculator3.Expose(1));

            Gain gain3 = new Gain();
            nodes.Add(gain3);
            conf["value"] = 1/m;
            calculator3.Connect(gain3);
            gain3.Connect(xddot);

            Scope xddot_sc = new Scope();
            nodes.Add(xddot_sc);
            xddot_sc.Name = "xddot_sc";
            xddot.Connect(xddot_sc);

            Inspector inspector = new Inspector();
            List<Model> routine = inspector.Inspect(nodes);

            Wanderer wanderer = new Wanderer();
            wanderer.Wander(routine);
        }
    }
}