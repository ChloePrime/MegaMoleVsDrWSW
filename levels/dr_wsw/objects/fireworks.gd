extends Node2D

@export var layer_0_speed := 300.0
@export var layer_1_speed := 400.0
const SimpleDanmaku = preload("res://levels/dr_wsw/objects/skill0/SimpleDanmaku.cs")
const DEG45 = deg_to_rad(45)


func _ready():
	for i in range(8):
		(get_child(i + 0) as SimpleDanmaku).Velocity = Vector2.RIGHT.rotated(i * DEG45) * layer_0_speed
		(get_child(i + 8) as SimpleDanmaku).Velocity = Vector2.RIGHT.rotated(i * DEG45) * layer_1_speed


func _on_child_exiting_tree(node: Node):
	if (get_child_count() == 1 && node.is_queued_for_deletion()):
		queue_free()
