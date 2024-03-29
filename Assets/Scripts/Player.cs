﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour {

	public float jumpvelocity;
	public float gravity;
	public Vector2 velocity;
	private bool walk, walk_left, walk_right, jump;
	public LayerMask wallMask = 0; // muss im Player-Inspektor auf Default gesetzt werden.
	public LayerMask floorMask; 

	public enum PlayerState {
		jumping,
		idle,
		walking
	}

	private PlayerState playerState = PlayerState.idle;
	private bool grounded = false;

	// Use this for initialization
	void Start () {
		Fall ();  // wird erst später aufgerufen, bis Kollision implementiert ist.
	}
	
	// Update is called once per frame
	void Update () {
		CheckPlayerInput ();
		UpdateAnimationStates ();
		UpdatePlayerPosition ();
	}

	void UpdatePlayerPosition() {

		Vector3 pos = transform.localPosition;
		Vector3 scale = transform.localScale;

		if (walk) {
			if (walk_left) {
				pos.x -= velocity.x * Time.deltaTime;
				scale.x = -1;
			}

			if (walk_right) {
				pos.x += velocity.x * Time.deltaTime;
				scale.x = 1;
			}

			/* 
			 * Das Attribut scale des Player-Objekts ist hier die Richtung [1 für rechts, -1 für links]
			 * Je nachdem, ob eine Kollision stattfand, ändert sich die Position je nach Laufrichtung oder
			 * sie bleibt an der Stelle, wo die Kollision stattfindet.
			 */
			pos = CheckWallRays (pos, scale.x); // BeBi
		}

		if (jump && (playerState != PlayerState.jumping)) {
			playerState = PlayerState.jumping;
			velocity = new Vector2 (velocity.x, jumpvelocity);
		}

		/* Geschwindigkeit der y-Bewegung wird kontinuierlich vermindert durch 
		 * die Gravitation, nachdem sie einmal beim Drücken der Jump-Taste 
		 * auf die Sprung-Geschwindigkeit gesetzt wurde (wird im Inspektor initialisiert).
		 * jumpvelocity auf 5 (x-velocity ist auf 7).
		 * Unbdeingt auch gravity setzen (auf 65).
		 * floorMask muss auf Default gesetzt werden, damit der Player nicht durch den 
		 * Boden fällt.
		 * Bereits zu Beginn würde das passieren, wenn wir Fall() direkt in Start() 
		 * aufrufen würden. Daher erst mal auskommentiert.
		 * Die z-Werte des Bodens und der Wände bleiben bei 27, weil sie sonst 
		 * unsichtbar würden. 
		 */
		if (playerState == PlayerState.jumping) {
			pos.y += velocity.y * Time.deltaTime;
			velocity.y -= gravity * Time.deltaTime;
		}

		if (velocity.y <= 0)  // Wir sparen uns die Öffnenden und Schließenden Klammern
			pos = CheckFloorRays (pos);

		if (velocity.y >= 0)
			pos = CheckCeilingRays (pos);
		
		transform.localPosition = pos;
		transform.localScale = scale;
	}

	void UpdateAnimationStates() {
		// idle
		if (grounded && !walk)	{

			GetComponent<Animator>().SetBool("isJumping", false);
			GetComponent<Animator>().SetBool("isRunning", false);
		}
		// walking (== running)
		if (grounded && walk) {
			
			GetComponent<Animator>().SetBool("isJumping", false);
			GetComponent<Animator>().SetBool("isRunning", true);
		}

		// jumping
		if (playerState == PlayerState.jumping) {
			GetComponent<Animator>().SetBool("isJumping", true);
			GetComponent<Animator>().SetBool("isRunning", false);
		}
	}

	void CheckPlayerInput () {

		// Hier können die Eingaben mittels GetKey abgefragt werden.
		bool input_left = Input.GetKey (KeyCode.LeftArrow);
		bool input_right = Input.GetKey (KeyCode.RightArrow);

		// Springen schon dann, wenn die Taste noch nicht losgelassen wurde.
		bool jump_key = Input.GetKeyDown (KeyCode.UpArrow);



		bool joy_buttonA = Input.GetButton ("Fire1");
		bool joy_buttonB = Input.GetButton ("Fire2");
		bool joy_buttonX = Input.GetButton ("Fire3");
		bool joy_buttonY = Input.GetButton ("Jump");
		bool joy_buttonHor = Input.GetButton ("Horizontal");
		bool joy_buttonVer = Input.GetButton ("Vertical");
		bool gamepad_A = UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.A);
		/*
		bool joy_button7 = Input.GetButton ("joystick button 7");
		bool joy_button8 = Input.GetButton ("joystick button 8");
		bool joy_button9 = Input.GetButton ("joystick button 9");
		bool joy_button10 = Input.GetButton ("joystick button 10");
		*/

		if (joy_buttonA) {
			Debug.Log ("Joystick Button A");
		}
		if (joy_buttonB) {
			Debug.Log ("Joystick Button B");
		}
		if (joy_buttonX) {
			Debug.Log ("Joystick Button X");
		}
		if (joy_buttonY) {
			Debug.Log ("Joystick Button Y");
		}
		if (joy_buttonHor) {
			Debug.Log ("Joystick Button Horizontal (Control-Cross)");
		}
		if (joy_buttonVer) {
			Debug.Log ("Joystick Button Vertical (Control-Cross)");
		}
		if (gamepad_A) {
			Debug.Log ("Gamepad Button A");
		}

		/*
		if (joy_button7) {
			Debug.Log ("Joystick Button 7");
		}
		if (joy_button8) {
			Debug.Log ("Joystick Button 8");
		}
		if (joy_button9) {
			Debug.Log ("Joystick Button 9");
		}
		if (joy_button10) {
			Debug.Log ("Joystick Button 10");
		}
		*/
		/*
		walk = input_left_joy || input_right_joy || input_left || input_right;
		walk_left = (input_left || input_left_joy) && !(input_right || input_right_joy);
		walk_right = (input_right || input_right_joy) && !(input_left || input_left_joy);
		jump = input_jump_joy || jump_key;
		*/

		/*
		 * OR-Operator wird benutzt, um zu sehen, ob entweder nach rechts ODER links gedrückt wurde.
		 * Funktioniert im Moment (Tutorial 3 / 12) nur mit den Cursor-Tasten. Im Emulator Citra kann man
		 * das passend belegen (auf die Buttons oder das Steuerkreuz).
		 * Später werden hier auch die Buttons und das Steuerkreuz vom 3DS abgefragt.
		 */ 
	
		walk = input_left || input_right || joy_buttonB || joy_buttonX;
		// Wenn nach links gedrückt wurde UND NICHT nach rechts 
		walk_left = ((input_left) && !(input_right)) || ((joy_buttonX) && !(joy_buttonB));
		
		// Wenn nach rechts gedrückt wurde UND NICHT nach links
		walk_right = ((input_right) && !(input_left)) || ((joy_buttonB) && !(joy_buttonX));
		// Wenn jump gedrückt wurde (Space oder später Y)
		jump = jump_key || joy_buttonY || gamepad_A;

	}

	void Fall() {

		// Player nach unten "ziehen". Springen ist = Fallen.
		velocity.y = 0;
		playerState = PlayerState.jumping;
		grounded = false;

	}

	Vector3 CheckWallRays(Vector3 pos, float direction) {

		/*
		 * Richtung ist entweder 1 (rechts) oder -1 (links). Die pos.x ist immer in der x-Mitte des Players.
		 * Daher wird die Richtung mit 0,4 multipliziert und zu der aktuellen Position addiert.
		 * Der Player ist eine Einheit breit.
		 * Das Ergebnis für die x-Position liegt noch knapp innerhalb des Spielers, 
		 * weil andernfalls Kollisionen entstünden, wenn gar keine vorhanden sind.
		 * Das gleiche gilt für die y-Position.  Der Player ist 2 Einheiten hoch. Es wird also 
		 * knapp am oberen Rand des Spielers "gemessen". 
		 */

		Vector2 originTop = new Vector2(pos.x + direction * 0.4f, pos.y + 1f - 0.2f);
		Vector2 originMiddle = new Vector2 (pos.x + direction * 0.4f, pos.y);
		/* 
		 * Ähnlich wie originTop, nur das bei y nun knapp 1 Einheit abgezogen werden muss.
		 * y wird nach unten hin abgezogen, noch oben hin addiert (ganz logisch).
		 */
		Vector2 originBottom = new Vector2(pos.x + direction * 0.4f, pos.y - 1f + 0.2f);

		/*
		 * Start ist die berechnete Stelle für Top / Middle und Bottom. Richtung ist die x-Richtung, 
		 * Entfernung richtet sich nach der Geschwindigkeit der Player-Bewegung.
		 * Man hätte das auch in ein Array packen können, um in einer Schleife alle verschiedenen Rays
		 * abfragen können. Aber das ist hier erst mal die einfachste Variante - ohne Optimierung.
		 * 
		 * Bei einer Kollision wird ein sog. Collider-Objekt erzeugt. Andernfalls ist es null.
		 * Es muss dann auch die Player-Position an der vorherigen Stelle bleiben 
		 * [in unserem Fall die Anzahl der Pixel, die sonst in die entsprechende Richtung gelaufen worden
		 * wäre], falls es zu einer Kollision kam.
		 * Das passiert, damit man nicht durch Objekte hindurchläuft.
		 */
		RaycastHit2D wallTop = Physics2D.Raycast (originTop, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);
		RaycastHit2D wallMiddle = Physics2D.Raycast (originMiddle, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);
		RaycastHit2D wallBottom = Physics2D.Raycast (originBottom, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);

		if (wallTop.collider != null || wallMiddle.collider != null || wallBottom.collider != null) {
			pos.x -= velocity.x * Time.deltaTime * direction;
		}

		/* Entweder die ursprünglich übergebene Position wird zurückgegeben (falls keine Kollision 
		 * stattfand). Oder die neu berechnete Position um die entsprechende Anzahl der Pixel korrigiert,
		 * damit man nicht durch Objekte läuft, sondern davor stehen bleibt. 
		 * Die y-Position wird momentan noch nicht geprüft. 
		 */

		return pos;
	}

	Vector3 CheckFloorRays(Vector3 pos) {
		/* Knapp innerhalb des Players ist die x-Position des Rays. Ebenso der y-Wert.
		 * Dieser ist am unteren Ende des Players.
		 * Ähnlich wie bei der Methode CheckWallRays().
		 */
		Vector2 originLeft = new Vector2 (pos.x - 0.5f + 0.2f, pos.y - 1f);
		Vector2 originMiddle = new Vector2 (pos.x, pos.y - 1f);
		Vector2 originRight = new Vector2 (pos.x + 0.5f - 0.2f, pos.y - 1f);

		/* 
		 * Vector2.down ist ein gleichwertig mit "new Vector2(0, -1)" und einfacher zu lesen
		 */

		RaycastHit2D floorLeft = Physics2D.Raycast (originLeft, Vector2.down, velocity.y * Time.deltaTime, floorMask);
		RaycastHit2D floorMiddle = Physics2D.Raycast (originMiddle, Vector2.down, velocity.y * Time.deltaTime, floorMask);
		RaycastHit2D floorRight = Physics2D.Raycast (originRight, Vector2.down, velocity.y * Time.deltaTime, floorMask);

		if (floorLeft.collider != null || floorMiddle.collider != null || floorRight.collider != null) {

			RaycastHit2D hitRay = floorRight;

			if (floorLeft) {
				hitRay = floorLeft;
			} else if (floorMiddle) {
				hitRay = floorMiddle;
			} else if (floorMiddle) {
				hitRay = floorMiddle;
			} 

			playerState = PlayerState.idle;
			grounded = true;
			velocity.y = 0;

			/*
			 * Quasi exakt oben auf einem Objekt landen, egal wo der hitRay getroffen hat.
			 * Die Höhe des Players ist 2. Also 1 addieren. Durch 2 teilen, weil wir die
			 * obere Stelle haben wollen.
			 */
			pos.y = hitRay.collider.bounds.center.y + hitRay.collider.bounds.size.y / 2 + 1;

		} else {
			
			/* Nur wenn wir nicht fallen bzw. springen, wird Fall() aufgerufen.
			 * Und das auch nur dann, wenn wir nichts nach unten hin berühren.
			 * Da Fallen = Springen, muss man das so machen. Andernfalls würde
			 * man mitten in einem Sprung stehen bleiben.
			 */
			if (playerState != PlayerState.jumping) {
				Fall ();
			}
		}

		return pos;
	}

	Vector3 CheckCeilingRays(Vector3 pos){
		/* Knapp innerhalb des Players ist die x-Position des Rays. Ebenso der y-Wert.
		 * Der y-Wert ist am oberen Ende des Players.
		 * Ähnlich wie bei der Methode CheckWallRays().
		 */
		Vector2 originLeft = new Vector2 (pos.x - 0.5f + 0.2f, pos.y + 1f);
		Vector2 originMiddle = new Vector2 (pos.x, pos.y + 1f);
		Vector2 originRight = new Vector2 (pos.x + 0.5f - 0.2f, pos.y + 1f);

		/* 
		 * Vector2.up ist ein gleichwertig mit "new Vector2(0, 1)" und einfacher zu lesen.
		 * Wir benutzen floorMask, weil es im Grunde keine Decke gibt und man an die gleichen 
		 * Objekte (nur von unten her) stößt.
		 */

		RaycastHit2D ceilLeft = Physics2D.Raycast (originLeft, Vector2.up, velocity.y * Time.deltaTime, floorMask);
		RaycastHit2D ceilMiddle = Physics2D.Raycast (originMiddle, Vector2.up, velocity.y * Time.deltaTime, floorMask);
		RaycastHit2D ceilRight = Physics2D.Raycast (originRight, Vector2.up, velocity.y * Time.deltaTime, floorMask);

		if (ceilLeft.collider != null || ceilMiddle.collider != null || ceilRight.collider != null) {

			RaycastHit2D hitRay = ceilLeft;

			if (ceilLeft) {
				hitRay = ceilLeft;
			} else if (ceilMiddle) {
				hitRay = ceilMiddle;
			} else if (ceilMiddle) {
				hitRay = ceilMiddle;
			} 

			/* Spieler wieder passend nach unten setzen, damit er nicht "im"
			 * angestoßenen Objekt angezeigt wird.
			 */
			pos.y = hitRay.collider.bounds.center.y - hitRay.collider.bounds.size.y / 2 - 1;

			Fall ();

		}

		return pos;
	}
}
