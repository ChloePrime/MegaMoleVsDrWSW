extends CharacterBody2D


static var SoundUtil := preload("res://scripts/util/SoundUtil.cs") as Script;


@export var gravity := 1000.0
@export var intital_velocity := Vector2(0, -400)
var slipped			:= false
var movement_halted	:= false
var lock_y			:= 0.0


func _ready():
	velocity = intital_velocity


func _physics_process(delta):
	if movement_halted:
		return
	
	if slipped or !is_on_floor():
		velocity.y += gravity * delta
	
	var collided: bool
	if slipped:
		global_position += velocity * delta
		collided = true
	else:
		collided = move_and_slide()

	if not collided:
		return
	
	if collided and not slipped:
		start_slipping()



func start_slipping():
	velocity = Vector2.ZERO
	slipped = true;
	movement_halted = true
	
	await get_tree().create_timer(1).timeout
	$"Slippery Gas".visible = true
	$"Slippery Gas".play("default")
	
	lock_y = global_position.y
	var tween := create_tween().set_loops(6)
	tween.tween_callback(self.clip_tween_callback).set_delay(0.2)


func clip_tween_callback():
	movement_halted = true
	await get_tree().create_timer(0.1).timeout
	movement_halted = false

	global_position.y = lock_y
	velocity = Vector2.ZERO


func _on_visible_on_screen_notifier_screen_exited():
	SoundUtil.Play(preload("res://resources/mario/ME_slippery_man.ogg"))
	await get_tree().create_timer(3.5).timeout
	finish_level()
	
	
func finish_level():
	SoundUtil.Play(preload("res://resources/level/ME_level_clear.ogg"))
	await get_tree().create_timer(8.0).timeout
	get_tree().quit()
