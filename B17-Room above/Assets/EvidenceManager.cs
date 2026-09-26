using UnityEngine;
using System.Collections.Generic;

public class EvidenceManager : MonoBehaviour
{
    public static EvidenceManager instance;
    public static int guiltScore = 0;

    void Awake()
    {
        instance = this;
        guiltScore = 0;
    }

    void Start()
    {
        SpawnEvidenceClues();
    }

    public void SpawnEvidenceClues()
    {
        SpawnWhiskeyBottleAndNote();
        SpawnTornPhotograph();
        SpawnDiaryPage();
    }

    // ==================== CLUE 1: WHISKEY BOTTLE & APOLOGY NOTE ====================
    private void SpawnWhiskeyBottleAndNote()
    {
        if (GameObject.Find("WhiskeyBottleClue") != null) return;

        // Find tv_desk or sofa_big - completely away from the gramophone on tea_table 1!
        GameObject targetFurniture = GameObject.Find("tv_desk");
        if (targetFurniture == null) targetFurniture = GameObject.Find("sofa_big");
        if (targetFurniture == null)
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.scene.isLoaded && (go.name.Contains("tv_desk") || go.name.Contains("sofa_big")))
                {
                    targetFurniture = go;
                    break;
                }
            }
        }

        Vector3 spawnPos = new Vector3(-8f, 0.75f, -38f);
        Transform parentTr = null;

        if (targetFurniture != null)
        {
            parentTr = targetFurniture.transform;
            Renderer rend = targetFurniture.GetComponentInChildren<Renderer>();
            GameObject tvObj = GameObject.Find("tv");
            Renderer tvRend = tvObj != null ? tvObj.GetComponentInChildren<Renderer>() : null;

            if (rend != null)
            {
                float tableTop = rend.bounds.max.y;
                Vector3 deskRight = targetFurniture.transform.right;
                Vector3 deskForward = targetFurniture.transform.forward;

                if (tvRend != null)
                {
                    // tv sits in the middle of tv_desk. Offset cleanly onto the open right tabletop wing
                    Vector3 tvCenter = tvRend.bounds.center;
                    spawnPos = tvCenter + (deskRight * 0.72f) - (deskForward * 0.04f);
                    spawnPos.y = tableTop + 0.005f;
                }
                else
                {
                    spawnPos = rend.bounds.center + (deskRight * 0.70f) - (deskForward * 0.04f);
                    spawnPos.y = tableTop + 0.005f;
                }
            }
            else
            {
                spawnPos = targetFurniture.transform.position + Vector3.up * 0.85f + targetFurniture.transform.right * 0.70f;
            }
        }

        // Bottle Root
        GameObject bottleObj = new GameObject("WhiskeyBottleClue");
        if (parentTr != null)
        {
            bottleObj.transform.SetParent(parentTr, true);
            bottleObj.transform.rotation = targetFurniture.transform.rotation;
        }
        bottleObj.transform.position = spawnPos;
        bottleObj.transform.localScale = Vector3.one * 0.85f;

        // Visual Mesh: Procedural Bottle
        MeshFilter mf = bottleObj.AddComponent<MeshFilter>();
        MeshRenderer mr = bottleObj.AddComponent<MeshRenderer>();
        mf.mesh = CreateBottleMesh();

        // Material: Dark amber / bourbon glass
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        Material bottleMat = new Material(litShader);
        bottleMat.color = new Color(0.25f, 0.11f, 0.03f, 0.92f);
        bottleMat.SetFloat("_Smoothness", 0.95f);
        bottleMat.SetFloat("_Metallic", 0.05f);
        mr.material = bottleMat;

        // Cork Stopper on top
        GameObject corkObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        corkObj.name = "BottleCork";
        corkObj.transform.SetParent(bottleObj.transform, false);
        corkObj.transform.localPosition = new Vector3(0f, 0.39f, 0f);
        corkObj.transform.localScale = new Vector3(0.045f, 0.02f, 0.045f);
        Destroy(corkObj.GetComponent<Collider>());
        Material corkMat = new Material(litShader);
        corkMat.color = new Color(0.65f, 0.50f, 0.35f, 1f);
        corkMat.SetFloat("_Smoothness", 0.15f);
        corkObj.GetComponent<MeshRenderer>().material = corkMat;

        // Bourbon Label on bottle body
        GameObject labelObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        labelObj.name = "BottleLabel";
        labelObj.transform.SetParent(bottleObj.transform, false);
        labelObj.transform.localPosition = new Vector3(0f, 0.13f, 0.071f);
        labelObj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        labelObj.transform.localScale = new Vector3(0.12f, 0.14f, 1f);
        Destroy(labelObj.GetComponent<Collider>());
        Material labelMat = new Material(litShader);
        labelMat.mainTexture = CreateBourbonLabelTexture();
        labelMat.SetFloat("_Smoothness", 0.2f);
        labelObj.GetComponent<MeshRenderer>().material = labelMat;

        // Collider & Interaction
        BoxCollider col = bottleObj.AddComponent<BoxCollider>();
        col.size = new Vector3(0.24f, 0.45f, 0.24f);
        col.center = new Vector3(0f, 0.22f, 0f);

        EvidenceItem ev = bottleObj.AddComponent<EvidenceItem>();
        ev.clueId = "whiskey_bottle";
        ev.promptActionText = "EXAMINE WHISKEY BOTTLE";
        ev.discoverySubtitle = "An empty bottle of bourbon... and a stained note in my handwriting: 'I'm sorry. I swear I didn't mean to. Please don't call them.'";

        // Apology Note prop beside bottle
        GameObject noteObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        noteObj.name = "ApologyNoteProp";
        noteObj.transform.SetParent(bottleObj.transform, false);
        noteObj.transform.localPosition = new Vector3(-0.16f, 0.005f, -0.02f);
        noteObj.transform.localRotation = Quaternion.Euler(90f, 15f, 0f);
        noteObj.transform.localScale = new Vector3(0.16f, 0.22f, 1f);
        Destroy(noteObj.GetComponent<Collider>()); // Bottle collider handles interaction

        Material noteMat = new Material(litShader);
        noteMat.mainTexture = CreateApologyNoteTexture();
        noteMat.SetFloat("_Smoothness", 0.15f);
        noteObj.GetComponent<MeshRenderer>().material = noteMat;
    }

    // ==================== CLUE 2: TORN PHOTOGRAPH ON DESK ====================
    private void SpawnTornPhotograph()
    {
        if (GameObject.Find("TornPhotoClue") != null) return;

        // Place on the desk next to cassette player
        GameObject desk = GameObject.Find("photo_frame_desk");
        if (desk == null) desk = GameObject.Find("photo_frame_desk (1)");
        if (desk == null)
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name.Contains("photo_frame_desk") && go.scene.isLoaded)
                {
                    desk = go;
                    break;
                }
            }
        }

        Vector3 spawnPos = new Vector3(-1.2f, 0.9f, 2f);
        Transform parentTr = null;

        if (desk != null)
        {
            parentTr = desk.transform;
            Renderer rend = desk.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                spawnPos = new Vector3(rend.bounds.center.x + 0.28f, rend.bounds.max.y + 0.005f, rend.bounds.center.z + 0.10f);
            }
            else
            {
                spawnPos = desk.transform.position + Vector3.up * 0.82f + Vector3.right * 0.35f;
            }
        }

        // Photo Root laying flat on table
        GameObject photoObj = new GameObject("TornPhotoClue");
        if (parentTr != null) photoObj.transform.SetParent(parentTr, true);
        photoObj.transform.position = spawnPos;
        photoObj.transform.localRotation = Quaternion.Euler(0f, 18f, 0f);

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        // Photo Paper Cardstock Backing (laying flat on table)
        GameObject paperBacking = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paperBacking.name = "PhotoPaperBacking";
        paperBacking.transform.SetParent(photoObj.transform, false);
        paperBacking.transform.localPosition = new Vector3(0f, 0.002f, 0f);
        paperBacking.transform.localScale = new Vector3(0.38f, 0.004f, 0.48f); // Larger, clear presence
        Destroy(paperBacking.GetComponent<Collider>());

        Material paperMat = new Material(litShader);
        paperMat.color = new Color(0.96f, 0.94f, 0.90f, 1f); // Vintage white photo border
        paperMat.SetFloat("_Smoothness", 0.30f);
        paperBacking.GetComponent<MeshRenderer>().material = paperMat;

        // Front Photo Face Quad (resting flat on top of the cardstock)
        GameObject photoPane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        photoPane.name = "PhotoPane";
        photoPane.transform.SetParent(photoObj.transform, false);
        photoPane.transform.localPosition = new Vector3(0f, 0.005f, 0f);
        photoPane.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Lay flat facing up
        photoPane.transform.localScale = new Vector3(0.34f, 0.44f, 1f);
        Destroy(photoPane.GetComponent<Collider>());

        Material photoMat = new Material(litShader);
        photoMat.mainTexture = CreateTornPhotoTexture();
        photoMat.SetFloat("_Smoothness", 0.45f); // Glossy photo print finish
        photoPane.GetComponent<MeshRenderer>().material = photoMat;

        // BoxCollider on root for easy aim & interaction
        BoxCollider col = photoObj.AddComponent<BoxCollider>();
        col.size = new Vector3(0.44f, 0.12f, 0.54f);
        col.center = new Vector3(0f, 0.04f, 0f);

        EvidenceItem ev = photoObj.AddComponent<EvidenceItem>();
        ev.clueId = "torn_photo";
        ev.promptActionText = "EXAMINE TORN PHOTOGRAPH";
        ev.discoverySubtitle = "A photograph from our anniversary trip... but my face has been violently gouged out with a razor blade. She couldn't stand to look at me.";
    }

    // ==================== CLUE 3: DIARY PAGE NEAR BED ====================
    private void SpawnDiaryPage()
    {
        if (GameObject.Find("DiaryPageClue") != null) return;

        GameObject bed = GameObject.Find("Bed");
        if (bed == null)
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name.ToLower().Contains("bed") && go.scene.isLoaded)
                {
                    bed = go;
                    break;
                }
            }
        }

        Vector3 spawnPos = new Vector3(-2f, 0.6f, -3f);
        Transform parentTr = null;

        if (bed != null)
        {
            parentTr = bed.transform;
            Renderer rend = bed.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                // Headboard is at rend.bounds.max.y; mattress surface is roughly ~38% of total bed height
                float mattressY = rend.bounds.min.y + (rend.bounds.size.y * 0.38f);
                Vector3 bedCenter = rend.bounds.center;
                
                // Position resting on the duvet/bedspread in the lower-middle half of the bed
                Vector3 targetBedPos = bedCenter + (bed.transform.right * 0.25f) + (bed.transform.forward * 0.20f);
                
                // Raycast down to snap directly onto the mattress/blanket surface if colliders exist
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(targetBedPos.x, rend.bounds.max.y + 0.5f, targetBedPos.z), Vector3.down, out hit, 4.0f))
                {
                    if (hit.point.y > rend.bounds.min.y + 0.15f)
                    {
                        mattressY = hit.point.y;
                    }
                }
                
                spawnPos = new Vector3(targetBedPos.x, mattressY + 0.008f, targetBedPos.z);
            }
            else
            {
                spawnPos = bed.transform.position + Vector3.up * 0.45f + Vector3.right * 0.30f;
            }
        }

        GameObject diaryObj = new GameObject("DiaryPageClue");
        if (parentTr != null) diaryObj.transform.SetParent(parentTr, true);
        diaryObj.transform.position = spawnPos;
        diaryObj.transform.localRotation = (parentTr != null) ? parentTr.rotation * Quaternion.Euler(0f, 15f, 0f) : Quaternion.Euler(0f, -22f, 0f);

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        // Leather Cover Backing
        GameObject diaryCover = GameObject.CreatePrimitive(PrimitiveType.Cube);
        diaryCover.name = "DiaryCover";
        diaryCover.transform.SetParent(diaryObj.transform, false);
        diaryCover.transform.localPosition = new Vector3(0f, 0.003f, 0f);
        diaryCover.transform.localScale = new Vector3(0.32f, 0.008f, 0.40f);
        Destroy(diaryCover.GetComponent<Collider>());
        Material coverMat = new Material(litShader);
        coverMat.color = new Color(0.18f, 0.12f, 0.09f, 1f); // Dark leather
        coverMat.SetFloat("_Smoothness", 0.3f);
        diaryCover.GetComponent<MeshRenderer>().material = coverMat;

        // Open Diary Parchment Page
        GameObject diaryPage = GameObject.CreatePrimitive(PrimitiveType.Quad);
        diaryPage.name = "DiaryPage";
        diaryPage.transform.SetParent(diaryObj.transform, false);
        diaryPage.transform.localPosition = new Vector3(0f, 0.008f, 0f);
        diaryPage.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        diaryPage.transform.localScale = new Vector3(0.28f, 0.36f, 1f);
        Destroy(diaryPage.GetComponent<Collider>());

        Material pageMat = new Material(litShader);
        pageMat.mainTexture = CreateDiaryPageTexture();
        pageMat.SetFloat("_Smoothness", 0.15f);
        diaryPage.GetComponent<MeshRenderer>().material = pageMat;

        // Interaction Collider
        BoxCollider col = diaryObj.AddComponent<BoxCollider>();
        col.size = new Vector3(0.38f, 0.14f, 0.46f);
        col.center = new Vector3(0f, 0.04f, 0f);

        EvidenceItem ev = diaryObj.AddComponent<EvidenceItem>();
        ev.clueId = "diary_page";
        ev.promptActionText = "READ DIARY PAGE";
        ev.discoverySubtitle = "Her diary from that night: 'He took my car keys. He changed the front door lock. He says he's protecting me... God help me.'";
    }

    // ==================== DYNAMIC ENDINGS BASED ON GUILT SCORE ====================
    public static string GetEndingTitle()
    {
        if (guiltScore == 0) return "ENDING 1: DENIAL";
        if (guiltScore == 1) return "ENDING 2: FRAGMENTS";
        if (guiltScore == 2) return "ENDING 3: AWAKENING";
        return "ENDING 4: FULL CONFESSION";
    }

    public static string GetEndingText()
    {
        if (guiltScore == 0)
        {
            return "<size=125%><b>ENDING 1: DENIAL</b></size>\n\n" +
                   "<size=68%>You escaped through the white light.\n" +
                   "You chose not to look at the evidence. You chose to remember nothing.\n\n" +
                   "<i>You can unlock the door... but you will never escape yourself.</i></size>";
        }
        else if (guiltScore == 1)
        {
            return "<size=125%><b>ENDING 2: FRAGMENTS</b></size>\n\n" +
                   "<size=68%>You saw a piece of the truth.\n" +
                   "The broken promises. The apologies. But you ran before you could face it all.\n\n" +
                   "<i>The memories you suppress do not disappear. They wait in the dark.</i></size>";
        }
        else if (guiltScore == 2)
        {
            return "<size=125%><b>ENDING 3: AWAKENING</b></size>\n\n" +
                   "<size=68%>The truth has formed.\n" +
                   "The empty bottle. The scratched photo. You are beginning to remember what happened.\n\n" +
                   "<i>You were never trapped here by someone else. You locked the door from the inside.</i></size>";
        }
        else
        {
            return "<size=125%><b>ENDING 4: FULL CONFESSION</b></size>\n\n" +
                   "<size=68%>You remember everything now.\n" +
                   "The shouting. The shattered glass. Her diary. The knife.\n\n" +
                   "<b>You did this.</b>\n\n" +
                   "<i>The room was never your prison. It was your guilt.</i></size>";
        }
    }

    // ==================== PROCEDURAL TEXTURES FOR REALISTIC PROPS ====================
    private Texture2D CreateTornPhotoTexture()
    {
        int width = 256;
        int height = 256;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color vintageBg = new Color(0.85f, 0.80f, 0.72f, 1f);
        Color silhouetteColor = new Color(0.22f, 0.18f, 0.16f, 1f);
        Color scratchColor = new Color(0.12f, 0.08f, 0.08f, 1f);

        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = (float)x / width - 0.5f;
                float dy = (float)y / height - 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float vig = Mathf.Clamp01(1f - dist * 0.7f);
                Color c = vintageBg * vig;

                // Wife silhouette on right side (center x = 160)
                float wDist = Mathf.Sqrt(Mathf.Pow(x - 160, 2) + Mathf.Pow(y - 140, 2));
                if (wDist < 38f || (x >= 140 && x <= 180 && y < 140 && y > 30))
                {
                    c = Color.Lerp(c, silhouetteColor, 0.85f);
                }

                // Husband silhouette on left side (center x = 95)
                float hDist = Mathf.Sqrt(Mathf.Pow(x - 95, 2) + Mathf.Pow(y - 150, 2));
                if (hDist < 42f || (x >= 70 && x <= 120 && y < 150 && y > 30))
                {
                    c = Color.Lerp(c, silhouetteColor, 0.85f);
                }

                // Violent razor scratches & gouges across the husband's side
                if (x < 130)
                {
                    if (Mathf.Abs((x * 1.4f + y) % 32 - 16) < 3.2f ||
                        Mathf.Abs((y * 1.2f - x) % 28 - 14) < 2.8f ||
                        (x > 80 && x < 110 && y > 130 && y < 170))
                    {
                        c = scratchColor;
                    }
                }

                // Torn / ripped jagged border down the center
                float tearWave = Mathf.Sin(y * 0.15f) * 6f + Mathf.Cos(y * 0.35f) * 4f;
                if (Mathf.Abs(x - (128 + tearWave)) < 2.5f)
                {
                    c = new Color(0.96f, 0.94f, 0.90f, 1f); // torn paper edge
                }

                pixels[y * width + x] = c;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private Texture2D CreateApologyNoteTexture()
    {
        int width = 256;
        int height = 256;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color paperColor = new Color(0.92f, 0.89f, 0.82f, 1f);
        Color inkColor = new Color(0.18f, 0.18f, 0.22f, 0.95f);
        Color stainColor = new Color(0.65f, 0.50f, 0.32f, 0.5f);

        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color c = paperColor;

                // Bourbon cup / glass stain ring
                float distRing = Mathf.Sqrt(Mathf.Pow(x - 170, 2) + Mathf.Pow(y - 90, 2));
                if (distRing >= 40f && distRing <= 46f)
                {
                    c = Color.Lerp(c, stainColor, 0.7f);
                }

                // Ruled handwriting lines across the note
                for (int lineY = 60; lineY <= 210; lineY += 25)
                {
                    float noise = Mathf.Sin(x * 0.4f) * 1.5f;
                    if (x > 30 && x < 225 && Mathf.Abs(y - (lineY + noise)) <= 1.2f)
                    {
                        if ((x % 14) > 3)
                        {
                            c = Color.Lerp(c, inkColor, 0.85f);
                        }
                    }
                }

                pixels[y * width + x] = c;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private Texture2D CreateBourbonLabelTexture()
    {
        int width = 256;
        int height = 128;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color labelCream = new Color(0.88f, 0.84f, 0.75f, 1f);
        Color goldBorder = new Color(0.65f, 0.48f, 0.20f, 1f);
        Color blackInk = new Color(0.12f, 0.10f, 0.10f, 1f);

        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color c = labelCream;

                // Border
                if (x < 6 || x > width - 7 || y < 6 || y > height - 7 ||
                    x == 12 || x == width - 13 || y == 12 || y == height - 13)
                {
                    c = goldBorder;
                }

                // Text bands simulation ("KENTUCKY STRAIGHT BOURBON - 1978")
                if ((y >= 75 && y <= 85 && x >= 40 && x <= 215) ||
                    (y >= 50 && y <= 62 && x >= 60 && x <= 195) ||
                    (y >= 25 && y <= 32 && x >= 85 && x <= 170))
                {
                    if ((x % 10) > 2) c = blackInk;
                }

                pixels[y * width + x] = c;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private Texture2D CreateDiaryPageTexture()
    {
        int width = 256;
        int height = 256;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color parchment = new Color(0.93f, 0.89f, 0.80f, 1f);
        Color marginRed = new Color(0.75f, 0.30f, 0.30f, 0.8f);
        Color lineBlue = new Color(0.60f, 0.70f, 0.80f, 0.5f);
        Color inkScript = new Color(0.15f, 0.15f, 0.20f, 0.9f);

        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color c = parchment;

                // Red vertical margin
                if (x == 45) c = marginRed;

                // Horizontal faint ruled lines
                if (y % 18 == 0 && x > 20 && x < 235) c = lineBlue;

                // Handwriting script
                for (int line = 36; line < 230; line += 18)
                {
                    if (x > 55 && x < 225)
                    {
                        float wiggle = Mathf.Sin(x * 0.35f + line) * 1.8f;
                        if (Mathf.Abs(y - (line + 4 + wiggle)) < 1.2f && (x % 12) > 2)
                        {
                            c = inkScript;
                        }
                    }
                }

                // Tear drop / red ink smudge
                float tearDist = Mathf.Sqrt(Mathf.Pow(x - 180, 2) + Mathf.Pow(y - 70, 2));
                if (tearDist < 16f)
                {
                    c = Color.Lerp(c, new Color(0.55f, 0.15f, 0.15f, 0.7f), Mathf.Clamp01(1f - tearDist / 16f));
                }

                pixels[y * width + x] = c;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    // ==================== PROCEDURAL BOTTLE MESH ====================
    private Mesh CreateBottleMesh()
    {
        Mesh mesh = new Mesh();
        int segments = 16;
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        // Heights and radii for realistic bourbon bottle silhouette
        float[] heights = new float[] { 0f, 0.04f, 0.22f, 0.28f, 0.38f };
        float[] radii = new float[] { 0.07f, 0.072f, 0.07f, 0.025f, 0.027f };

        for (int h = 0; h < heights.Length; h++)
        {
            float y = heights[h];
            float r = radii[h];
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                verts.Add(new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
            }
        }

        int stride = segments + 1;
        for (int ring = 0; ring < heights.Length - 1; ring++)
        {
            for (int i = 0; i < segments; i++)
            {
                int cur = ring * stride + i;
                int next = cur + stride;

                tris.Add(cur);
                tris.Add(next);
                tris.Add(cur + 1);

                tris.Add(cur + 1);
                tris.Add(next);
                tris.Add(next + 1);
            }
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
