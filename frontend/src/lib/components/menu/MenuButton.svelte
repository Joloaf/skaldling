<script lang="ts">
	import Icon from '$lib/components/Icon.svelte';

	let {
		title,
		subtitle = '',
		href = '',
		icon = '',
		disabled = false
	}: {
		title: string;
		subtitle?: string;
		href?: string;
		icon?: string;
		disabled?: boolean;
	} = $props();

	const SPARK_PALETTE = ['var(--accent)', 'var(--gold)', 'var(--accent-cool)', 'var(--accent)'];

	function hash(seed: number): number {
		const v = Math.sin(seed) * 43758.5453;
		return v - Math.floor(v);
	}

	const SPARK_COUNT = 16;
	const sparks = Array.from({ length: SPARK_COUNT }, (_, i) => {
		const angle = (i / SPARK_COUNT) * Math.PI * 2;
		const spread = 0.9 + hash(i * 1.7) * 0.35;
		const drift = 8 + hash(i * 3.1) * 14;
		return {
			x: +(50 + Math.cos(angle) * 52 * spread).toFixed(2),
			y: +(50 + Math.sin(angle) * 66 * spread).toFixed(2),
			dx: +(Math.cos(angle) * drift).toFixed(2),
			dy: +(Math.sin(angle) * drift).toFixed(2),
			size: +(2.5 + hash(i * 4.3) * 3.5).toFixed(2),
			delay: +(hash(i * 5.9) * 1.6).toFixed(2),
			dur: +(1.5 + hash(i * 7.3) * 1.4).toFixed(2),
			color: SPARK_PALETTE[i % SPARK_PALETTE.length]
		};
	});
</script>

{#if disabled}
	<div class="menu-button" aria-disabled="true">
		{#if icon}<span class="icon" aria-hidden="true"><Icon name={icon} size={22} /></span>{/if}
		<span class="text">
			<span class="title">{title}</span>
			{#if subtitle}<span class="subtitle">{subtitle}</span>{/if}
		</span>
	</div>
{:else}
	<a class="menu-button" {href}>
		<span class="sparkles" aria-hidden="true">
			{#each sparks as spark, i (i)}
				<span
					class="spark"
					style="--x:{spark.x}%; --y:{spark.y}%; --dx:{spark.dx}px; --dy:{spark.dy}px; --size:{spark.size}px; --delay:{spark.delay}s; --dur:{spark.dur}s; --c:{spark.color}"
				></span>
			{/each}
		</span>
		{#if icon}<span class="icon" aria-hidden="true"><Icon name={icon} size={22} /></span>{/if}
		<span class="text">
			<span class="title">{title}</span>
			{#if subtitle}<span class="subtitle">{subtitle}</span>{/if}
		</span>
	</a>
{/if}

<style>
    .menu-button {
        display: flex;
        align-items: center;
        gap: 1rem;
        width: 100%;
        padding: 1rem 1.25rem;
        border: 1px solid var(--border, #e6dcc4);
        border-radius: var(--radius, 12px);
        background: var(--surface, #fbf5e9);
        color: var(--text, #2a2018);
        text-decoration: none;
        text-align: left;
        box-sizing: border-box;
        position: relative;
        isolation: isolate;
        transition: box-shadow 0.2s ease, border-color 0.2s ease;
    }
    a.menu-button:hover,
    a.menu-button:focus-visible {
        border-color: var(--accent, #46aab0);
        box-shadow:
                0 0 0 1px var(--accent, #46aab0),
                0 6px 28px color-mix(in srgb, var(--accent, #46aab0) 40%, transparent);
    }

    .sparkles {
        position: absolute;
        inset: 0;
        z-index: -1;
        pointer-events: none;
        opacity: 0;
        transition: opacity 0.3s ease;
    }
    a.menu-button:hover .sparkles,
    a.menu-button:focus-visible .sparkles {
        opacity: 1;
    }
    .spark {
        position: absolute;
        left: var(--x);
        top: var(--y);
        width: var(--size);
        height: var(--size);
        margin: calc(var(--size) * -0.5);
        border-radius: 50%;
        background: radial-gradient(
            circle,
            var(--c) 0%,
            color-mix(in srgb, var(--c) 55%, transparent) 45%,
            transparent 72%
        );
        box-shadow:
            0 0 6px var(--c),
            0 0 12px color-mix(in srgb, var(--c) 50%, transparent);
        opacity: 0;
        transform: scale(0);
        animation: spark-twinkle var(--dur) ease-in-out var(--delay) infinite;
        animation-play-state: paused;
    }
    a.menu-button:hover .spark,
    a.menu-button:focus-visible .spark {
        animation-play-state: running;
    }

    @keyframes spark-twinkle {
        0% {
            opacity: 0;
            transform: translate(0, 0) scale(0.2);
        }
        35% {
            opacity: 1;
            transform: translate(calc(var(--dx) * 0.5), calc(var(--dy) * 0.5)) scale(1);
        }
        100% {
            opacity: 0;
            transform: translate(var(--dx), var(--dy)) scale(0.35);
        }
    }

    @media (prefers-reduced-motion: reduce) {
        .sparkles {
            display: none;
        }
    }
    a.menu-button:focus-visible {
        outline: 2px solid var(--accent, #46aab0);
        outline-offset: 2px;
    }
    .menu-button[aria-disabled='true'] {
        opacity: 0.5;
        cursor: not-allowed;
    }

    .icon {
        flex: none;
        display: grid;
        place-items: center;
        width: 44px;
        height: 44px;
        border-radius: 10px;
        background: var(--accent-tint, rgba(91, 122, 58, 0.14));
        color: var(--accent, #46aab0);
    }

    .text { display: flex; flex-direction: column; }
    .title { font-weight: 700; font-size: 1.1rem; }
    .subtitle { font-size: 0.85rem; color: var(--text-muted, #6b5d4a); margin-top: 0.1rem; }
</style>