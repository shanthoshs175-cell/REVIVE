EXTERNAL has_observed(id)
VAR complete = false
-> arrival
=== arrival ===
You arrive at the scene. A person appears to be down, and you notice something unusual near electrical equipment. What should you do first?
+ [Immediately approach the person. # action:approach]
    -> END
+ [Stay at a safe distance and assess the scene. # action:assess]
    -> END
+ [Touch the electrical equipment to determine whether it is dangerous. # action:touch]
    -> END
+ [Leave the scene. # action:leave]
    -> END
=== help ===
You have identified a possible electrical hazard, and the area has not been made safe. What should you do next?
+ [Enter the area and reach the person. # action:enter]
    -> END
+ [Ask a bystander to enter the area. # action:send]
    -> END
+ [Stay outside the hazard area and call for appropriate help. # action:call]
    -> END
+ [Try to handle the electrical equipment yourself. # action:handle]
    -> END
=== monitor ===
Help has been requested, but the hazard has still not been controlled. What should you do?
+ [Enter the area because help has already been called. # action:enter_after_call]
    -> END
+ [Stay at a safe distance and continue observing the situation. # action:observe]
    -> END
+ [Ask an untrained person to enter the area. # action:send_again]
    -> END
+ [Touch the equipment again to check whether the hazard is gone. # action:touch_again]
    -> END
=== approach_feedback ===
You enter an area that has not been assessed. The demonstration shows the risk to you. Stay outside and assess the scene before direct help.
-> END
=== assess_feedback ===
You assess the scene without entering. The damaged equipment is identified as a hazard; the area has not been made safe.
-> END
=== touch_feedback ===
Approaching and reaching toward the suspected hazard puts you at risk. Assess from outside; touching it is not a scene-safety check.
-> END
=== leave_feedback ===
You walk away without initiating assistance. From the outside position shown in this scenario, you can assess the danger and request appropriate help without entering.
-> END
=== enter_feedback ===
The hazard has not been controlled. Entering puts you at risk. Remain outside and request appropriate help.
-> END
=== send_feedback ===
The bystander approaches the hazard and stops. Scene assessment includes risks to other people; do not direct them into this unsafe area.
-> END
=== call_feedback ===
You request appropriate help while remaining outside. Assistance has been requested; this does not mean the hazard is controlled.
-> END
=== handle_feedback ===
You move toward the uncontrolled hazard and pull away. Stay outside and request appropriate assistance instead of handling it.
-> END
=== enter_after_call_feedback ===
Calling for help does not make the scene safe. The uncontrolled hazard still puts you at risk if you enter.
-> END
=== observe_feedback ===
You remain outside and continue observing while help is requested. Maintain awareness of your surroundings and your own safety; the hazard remains uncontrolled.
-> END
=== send_again_feedback ===
Another person approaches the hazard and stops. Help being requested does not make entry safe for bystanders.
-> END
=== touch_again_feedback ===
The hazard has not been controlled. Reaching toward the equipment is not a safe way to check it. Remain outside.
-> END
