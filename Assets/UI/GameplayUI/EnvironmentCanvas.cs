using UnityEngine;
using Revive.Scenarios;
using UnityEngine.UIElements;
namespace Revive.Presentation
{
    /// <summary>Original vector environment. Presentation only: no medical or decision logic.</summary>
    public sealed class EnvironmentCanvas : VisualElement
    {
        public float Incident { get; set; }
        public float Reaction { get; set; }
        public bool Warning { get; set; }
        public bool ReducedMotion { get; set; }
        public WorldResponse? Response { get; set; }
        public float ResponseProgress { get; set; }
        public bool AssessmentVisible { get; set; }
        public bool CommunicationVisible { get; set; }
        private Painter2D pen;
        private float sx, sy;
        public EnvironmentCanvas() { generateVisualContent += Paint; pickingMode = PickingMode.Ignore; }
        private static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }
        private Vector2 V(float x, float y) => new Vector2(x * sx, y * sy);
        private void Polygon(string color, params Vector2[] points)
        {
            pen.fillColor = C(color); pen.BeginPath(); pen.MoveTo(V(points[0].x, points[0].y));
            for (int i = 1; i < points.Length; i++) pen.LineTo(V(points[i].x, points[i].y));
            pen.ClosePath(); pen.Fill();
        }
        private void Rect(float x, float y, float w, float h, string color) => Polygon(color, new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h));
        private void Line(float x, float y, float x2, float y2, string color, float width = 2)
        { pen.strokeColor = C(color); pen.lineWidth = width * sx; pen.BeginPath(); pen.MoveTo(V(x,y)); pen.LineTo(V(x2,y2)); pen.Stroke(); }
        private void Circle(float x, float y, float radius, string color)
        { pen.fillColor = C(color); pen.BeginPath(); pen.Arc(V(x,y),radius*sx,0,360); pen.Fill(); }
        private void Paint(MeshGenerationContext context)
        {
            if (contentRect.width < 1) return;
            pen = context.painter2D; sx = contentRect.width / 1000; sy = contentRect.height / 640;
            Rect(0,0,1000,640,"152A36");
            // Layered concourse wall, architectural glazing and daylight.
            Rect(0,35,1000,240,"203C49");
            for (int i=0;i<6;i++)
            {
                Rect(25+i*172,40,145,190,"345664"); Rect(30+i*172,45,134,100,"456C78");
                Rect(36+i*172,55,120,40,"527A83");
                Line(25+i*172,145,170+i*172,145,"233F4C",5);
            }
            Polygon("254654",new Vector2(0,240),new Vector2(1000,240),new Vector2(1000,310),new Vector2(0,285));
            Polygon("34434B",new Vector2(0,285),new Vector2(1000,285),new Vector2(1000,640),new Vector2(0,640));
            // Perspective floor seams.
            for (int i=-5;i<12;i++) Line(500+i*65,285,500+i*190,640,"41535B",1);
            foreach (float y in new[]{320f,367,426,500,592}) Line(0,y,1000,y,"42555D",1);
            Polygon("3C535C",new Vector2(90,286),new Vector2(205,286),new Vector2(60,640),new Vector2(0,640));
            Polygon("425961",new Vector2(440,286),new Vector2(528,286),new Vector2(790,640),new Vector2(590,640));
            // Structural columns and wall signage.
            Rect(0,0,30,355,"142C38"); Rect(200,0,18,285,"18323F"); Rect(850,0,24,290,"18323F");
            Rect(235,180,180,38,"17363E"); Rect(250,192,55,5,"72B4AC"); Rect(250,204,125,3,"4C7F80");
            Line(385,194,398,200,"A4CEC2",3); Line(398,200,385,206,"A4CEC2",3);
            Rect(700,170,115,55,"1E343E"); Rect(710,180,4,34,"DFAD65");
            Rect(725,183,75,5,"89999A"); Rect(725,197,52,3,"657E83");
            // Maintenance bay and equipment; no implied route across the spill.
            Polygon("293840",new Vector2(548,330),new Vector2(890,315),new Vector2(955,510),new Vector2(530,515));
            Line(538,510,955,510,"C39A5A",5);
            for(int i=0;i<13;i++) Line(550+i*31,506,559+i*31,510,"273B44",6);
            Rect(740,230,76,115,"1B2B34"); Rect(747,237,62,90,"647375");
            Rect(754,245,48,37,"273F4A"); Circle(788,300,5,"D4A36B");
            for(int i=0;i<4;i++) Rect(755,290+i*9,22,2,"3F545B");
            Line(781,345,745,367,"12262F",6); Line(745,367,695,355,"12262F",6);
            Line(695,355,670,380,"12262F",6); Line(670,380,645,379,"B6A181",3);
            Polygon("526A71",new Vector2(665,385),new Vector2(717,372),new Vector2(795,388),new Vector2(816,408),new Vector2(755,432),new Vector2(665,420),new Vector2(625,400));
            Line(680,393,750,384,"759397",2); Line(744,417,784,406,"759397",2);
            // Warning A-frame sign.
            Polygon("CA9B58",new Vector2(867,360),new Vector2(850,428),new Vector2(910,428),new Vector2(892,360));
            Polygon("E6BA74",new Vector2(857,417),new Vector2(869,367),new Vector2(884,367),new Vector2(899,417));
            Polygon("2C4047",new Vector2(878,375),new Vector2(865,402),new Vector2(891,402));
            Line(878,383,878,393,"E6BA74",3); Circle(878,398,1.5f,"E6BA74");
            // Actor motion is selected by presentation data, never by question IDs.
            float t = ResponseProgress;
            float travel = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / .48f));
            float retreat = Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - .62f) / .3f));
            float excursion = travel * (1 - .3f * retreat);
            bool directing = Response == WorldResponse.DirectBystander;
            float bx = directing ? Mathf.Lerp(332, 550, ReducedMotion ? .7f : excursion) : 332;
            float by = directing ? Mathf.Lerp(292, 458, ReducedMotion ? .7f : excursion) : 292;
            Person(bx,by,directing && t > .6f ? -12 * retreat : 0,.71f,"728989","C7A68E",directing && !ReducedMotion ? Mathf.Sin(t*32)*travel*(1-retreat) : 0);
            Person(393,294,0,.68f,"596B7B","B9957E");
            // A restrained, non-graphic fall, driven by the Animator.
            float fall = Mathf.SmoothStep(0,1,Incident);
            Person(573 + fall*40,346 + fall*44,-fall*78,1,"B2926D","C4A68F");
            // Entrance standing zone, not a claim that the incident area is safe.
            Polygon("2C565A",new Vector2(60,493),new Vector2(290,493),new Vector2(374,620),new Vector2(0,620));
            Line(60,493,290,493,"74B5AD",3); Line(290,493,374,620,"74B5AD",3);
            for(int i=0;i<3;i++){Line(80+i*42,579,93+i*42,567,"6A9F9D",3);Line(93+i*42,567,106+i*42,579,"6A9F9D",3);}
            // Player responder: distinct teal jacket, grounded shadow and articulated gait.
            bool approach = Response == WorldResponse.Approach;
            bool reach = Response == WorldResponse.Reach;
            bool leaving = Response == WorldResponse.Leave;
            float movement = ReducedMotion ? .7f : excursion;
            float px = approach ? Mathf.Lerp(265,595,movement) : reach ? Mathf.Lerp(265,709,movement) : leaving ? Mathf.Lerp(265,35,ReducedMotion?1:travel) : 265;
            float py = approach ? Mathf.Lerp(554,468,movement) : reach ? Mathf.Lerp(554,401,movement) : leaving ? Mathf.Lerp(554,620,ReducedMotion?1:travel) : 554;
            bool locomotion = approach || reach || leaving;
            float gait = locomotion && !ReducedMotion && t < .6f ? Mathf.Sin(t*38)*travel : 0;
            float recoil = (approach || reach) && t > .6f ? -18*retreat : 0;
            Circle(px,py+40,30,"203B43");
            Person(px,py,recoil,.95f,"62B8AC","D3AC91",gait,reach && t>.4f && t<.72f || directing,Response==WorldResponse.Call || CommunicationVisible);
            // A single restrained hazard pulse. Reduced motion uses a steady amber symbol.
            bool electrical = approach || reach || directing;
            if (AssessmentVisible) { Line(731,224,820,224,"E6BA74",3); Line(820,224,820,347,"E6BA74",3); }
            if (electrical && (ReducedMotion || t>.48f && t<.76f))
            {
                string pulse = ReducedMotion ? "D9AD69" : "E9D8A5";
                Line(705,358,685,379,pulse,4); Line(685,379,701,378,pulse,4); Line(701,378,684,399,pulse,4);
                if (!ReducedMotion) { Line(724,367,736,351,pulse,2); Line(665,379,651,365,pulse,2); }
            }
            if (Response == WorldResponse.Survey || Response == WorldResponse.Monitor)
            {
                float scan = ReducedMotion ? .5f : Mathf.SmoothStep(0,1,t);
                Line(285,471,Mathf.Lerp(595,786,scan),365,"659E98",1);
                Circle(770,285,48,"284A52");
                Rect(748,247,49,72,"586F72"); Rect(754,253,36,28,"233C47");
                Line(740,334,802,334,"E8BD78",3);
            }
            if (CommunicationVisible && (Response != WorldResponse.Call || t>.45f))
            {
                Rect(385,493,111,49,"1B414A");
                for(int i=0;i<4;i++) Rect(399+i*14,527-i*7,7,8+i*7,"8BD4BE");
                Line(464,514,471,521,"B5E5CF",3); Line(471,521,485,505,"B5E5CF",3);
            }
            // Foreground railing adds depth without occluding the scene.
            Rect(0,606,1000,34,"142833"); Line(0,608,1000,608,"29444F",3);
            if (Warning && Reaction > .05f) Line(520,526,958,526,"EBA47E",4+Reaction*3);
            if (!ReducedMotion)
                for(int i=0;i<9;i++) { float y=80+(i*57+Time.unscaledTime*5)%430; Circle(100+i*99,y,1,"668188"); }
        }
        private void Person(float x,float y,float degrees,float scale,string clothes,string skin,float gait=0,bool reaching=false,bool phone=false)
        {
            float r=degrees*Mathf.Deg2Rad;
            Vector2 P(float a,float b) => new Vector2(x+(a*Mathf.Cos(r)-b*Mathf.Sin(r))*scale,y+(a*Mathf.Sin(r)+b*Mathf.Cos(r))*scale);
            void Limb(float a,float b,float c,float d,string color,float width) { var p=P(a,b);var q=P(c,d);Line(p.x,p.y,q.x,q.y,color,width*scale); }
            Limb(-8,-3,-17+gait*12,45,"1D303B",14);Limb(9,-3,21-gait*12,42,"213542",14);
            Limb(-17,44,-30,46,"142630",9);Limb(21,42,34,43,"142630",9);
            Polygon(clothes,P(-18,-60),P(17,-60),P(21,-7),P(-17,-7));
            Limb(-16,-51,-33,-17,clothes,10);Limb(-33,-17,-33,-9,skin,7);
            if (phone) { Limb(17,-50,25,-76,clothes,10); var device=P(22,-88); Rect(device.x,device.y,10*scale,18*scale,"122832"); }
            else if (reaching) { Limb(17,-50,51,-52,clothes,10);Limb(51,-52,60,-50,skin,7); }
            else { Limb(17,-50,32,-19,clothes,10);Limb(32,-19,35,-11,skin,7); }
            // Jacket seams and badge give each silhouette a clearer body/readable pose.
            Limb(0,-57,0,-12,"476F71",2);
            var neck=P(0,-66);Circle(neck.x,neck.y,7*scale,skin);
            var head=P(0,-82);Circle(head.x,head.y,14*scale,skin);
            var hair=P(-2,-90);Circle(hair.x,hair.y,12*scale,"29343A");
        }
    }
}
