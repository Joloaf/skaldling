<script lang="ts">
	import type { SpriteDto } from '$lib/api/sprites';
	import Icon from '$lib/components/Icon.svelte'

	type SelectionsMap = Record<string, string>;

	let {
		catalog,
		selections = $bindable(),
		heading = ''
	}: {
		catalog: SpriteDto[];
		selections: SelectionsMap;
		heading?: string;
	} = $props();

	const TYPE_DISPLAY_ORDER = [
		'BodyArchetype', 'Face', 'Eyes', 'Hair', 'OutfitTop', 'OutfitBottom', 'Accessory'
	];

	const LABELS: Record<string, string> = {
		BodyArchetype: 'Body',
		Face: 'Face',
		Eyes: 'Eyes',
		Hair: 'Hair',
		OutfitTop: 'Shirt',
		OutfitBottom: 'Pants',
		Accessory: 'Accessory'
	};

	function groupedByType(catalog: SpriteDto[]) {
		const groups: Record<string, SpriteDto[]> = {};
		for (const sprite of catalog) {
			(groups[sprite.type] ??= []).push(sprite);
		}
		for (const type in groups) {
			groups[type].sort((a, b) => a.name.localeCompare(b.name));
		}
		return groups;
	}

	let groups = $derived(groupedByType(catalog));

	function cycle(type: string, direction: -1 | 1) {
		const typeSprites = groups[type];
		if (!typeSprites || typeSprites.length === 0) return;
		const currentId = selections[type];
		const currentIndex = typeSprites.findIndex((s) => s.id === currentId);
		const startIndex = currentIndex < 0 ? 0 : currentIndex;
		const nextIndex = (startIndex + direction + typeSprites.length) % typeSprites.length;
		selections = { ...selections, [type]: typeSprites[nextIndex].id };
	}

	function randomize() {
		const newSelections: SelectionsMap = {};
		for (const type of TYPE_DISPLAY_ORDER) {
			const typeSprites = groups[type];
			if (!typeSprites || typeSprites.length === 0) continue;
			const random = typeSprites[Math.floor(Math.random() * typeSprites.length)];
			newSelections[type] = random.id;
		}
		selections = newSelections;
	}

	function selectedInLayerOrder() {
		return Object.values(selections)
			.map((id) => catalog.find((s) => s.id === id))
			.filter((s): s is SpriteDto => s !== undefined)
			.sort((a, b) => a.layer - b.layer);
	}

	function spriteForType(type: string): SpriteDto | undefined {
		const id = selections[type];
		return catalog.find((s) => s.id === id);
	}
</script>

<div class="avatar-builder">
	<div class="preview-col">
		{#if heading}<h2 class="ab-heading">{heading}</h2>{/if}
		<div class="preview">
			{#each selectedInLayerOrder() as sprite (sprite.id)}
				<img src={sprite.assetPath} alt={sprite.description} />
			{/each}
		</div>
		<button type="button" class="randomize" onclick={randomize}>
			<Icon name="dice" size={18} />
			Randomize
		</button>
	</div>

	<div class="parts">
		{#each TYPE_DISPLAY_ORDER as type (type)}
			{@const sprite = spriteForType(type)}
			{#if sprite}
				<div class="part-row">
					<span class="part-label">{LABELS[type] ?? type}</span>
					<button type="button" class="arrow" onclick={() => cycle(type, -1)} aria-label="Previous {LABELS[type] ?? type}">
						<Icon name="arrow-left" size={18} />
					</button>
					<span class="part-name">{sprite.name}</span>
					<button type="button" class="arrow" onclick={() => cycle(type, 1)} aria-label="Next {LABELS[type] ?? type}">
						<Icon name="arrow-right" size={18} />
					</button>
				</div>
			{/if}
		{/each}
	</div>
</div>

<style>
    .ab-heading {
        margin: 0;
        padding-top: 0.55rem;
        font-size: 1.1rem;
        font-weight: 700;
    }

    .avatar-builder {
        display: grid;
        grid-template-columns: 256px 1fr;
        gap: 1.75rem;
        align-items: start;
    }

    .preview-col {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
    }
    .preview {
        position: relative;
        width: 256px;
        height: 256px;
        border-radius: 14px;
        background: #efe6d3;
        border: 1px solid var(--border);
        box-shadow: inset 0 2px 8px rgba(0, 0, 0, 0.15);
        overflow: hidden;
    }
    .preview img {
        position: absolute;
        inset: 0;
        width: 100%;
        height: 100%;
    }

    .randomize {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 0.45rem;
        width: 256px;
        padding: 0.55rem;
        border: 1px solid var(--border);
        border-radius: 10px;
        background: var(--surface-alt, #3c315a);
        color: var(--text);
        font-weight: 600;
        cursor: pointer;
        transition: border-color 0.15s ease, color 0.15s ease, background 0.15s ease;
    }
    .randomize:hover {
        border-color: var(--accent);
        color: var(--accent);
    }

    .parts {
        display: flex;
        flex-direction: column;
    }
    .part-row {
        display: flex;
        align-items: center;
        gap: 0.6rem;
        padding: 0.55rem 0;
        border-bottom: 1px solid var(--border);
    }
    .part-label {
        width: 5.5rem;
        font-weight: 600;
    }
    .part-name {
        flex: 1;
        text-align: center;
        font-family: ui-monospace, monospace;
        color: var(--text-muted);
        font-size: 0.9rem;
    }
    .arrow {
        display: inline-grid;
        place-items: center;
        width: 32px;
        height: 32px;
        border: 1px solid var(--border);
        border-radius: 9px;
        background: var(--surface-alt, #3c315a);
        color: var(--text);
        cursor: pointer;
        transition: border-color 0.15s ease, color 0.15s ease, background 0.15s ease;
    }
    .arrow:hover {
        border-color: var(--accent);
        color: var(--accent);
        background: color-mix(in srgb, var(--accent) 12%, var(--surface-alt, #3c315a));
    }

    @media (max-width: 560px) {
        .avatar-builder {
            grid-template-columns: 1fr;
        }
        .preview,
        .randomize {
            width: 100%;
            max-width: 256px;
        }
    }
</style>
