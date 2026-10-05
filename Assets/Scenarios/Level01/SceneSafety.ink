EXTERNAL has_observed(id)
VAR complete = false
-> observe
=== observe ===
{ complete:
    Help has been requested. Your scene-safety assessment is complete; the hazard has not been declared safe.
    -> END
}
A person has fallen beside a maintenance bay. Pause at the entrance. What can you establish from here?
+ {not has_observed("equipment")} [Inspect the equipment from here # action:observe_equipment]
    -> END
+ {not has_observed("area")} [Look for a safer standing area # action:observe_area]
    -> END
+ {not has_observed("people")} [Observe people and the access route # action:observe_people]
    -> END
+ [Approach the person now # action:approach]
    -> END
+ [Stay back and request appropriate help # action:request_help]
    -> END
+ [Wait for someone else to take charge # action:wait]
    -> END
=== equipment_feedback ===
A damaged cable lies beside the spill. The hazard is not controlled. Keep outside this area.
-> END
=== area_feedback ===
The entrance provides a position away from the damaged equipment. A safe place to stand is not proof of a safe route to the person.
-> END
=== people_feedback ===
One person is on the ground. Two bystanders are outside the bay; the entrance is unobstructed. Include others in your scene assessment.
-> END
=== unsafe_feedback ===
The electrical hazard remains uncontrolled. Do not approach. Stay away and request appropriate help.
-> END
=== unassessed_feedback ===
You have not established a safe approach. Assess risks to yourself, the person and bystanders before entering.
-> END
=== help_feedback ===
You stay outside the hazardous area and request appropriate help. This is a safe response; the scene itself remains hazardous.
-> END
=== wait_feedback ===
Someone else may also be waiting. You can take a useful step now by requesting help from your safe position.
-> END
