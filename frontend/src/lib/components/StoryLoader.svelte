<script lang="ts">
	import { LOADING_LINES } from './loadingLines';

	let {
		subtitle = 'The skald is composing your saga — this can take a minute or two.'
	}: { subtitle?: string } = $props();

	// Shuffle the order of text lines from loadingLines using a Fisher-Yates algorithm.
	function shuffledIndexes(count: number): number[] {
		const indexes = Array.from({ length: count }, (_, index) => index);
		for (let current = indexes.length - 1; current > 0; current--) {
			const target = Math.floor(Math.random() * (current + 1));
			[indexes[current], indexes[target]] = [indexes[target], indexes[current]];
		}
		return indexes;
	}

	let order = $state(shuffledIndexes(LOADING_LINES.length));
	let lineIndex = $state(0);
	let swapping = $state(false);

	const line = $derived(LOADING_LINES[order[lineIndex]]);

	// Rotating through the loadingLines, changing every 5s with a 0.8s fade
	$effect(() => {
		let swapTimer: ReturnType<typeof setTimeout>;
		const cycle = setInterval(() => {
			swapping = true;
			swapTimer = setTimeout(() => {
				lineIndex = (lineIndex + 1) % order.length;
				swapping = false;
			}, 800);
		}, 5000);
		return () => {
			clearInterval(cycle);
			clearTimeout(swapTimer);
		};
	});
</script>

<div class="loader">
	<div class="stage" aria-hidden="true">
		<div class="glow"></div>

		<!-- Orbiting sparkles -->
		<div class="orbit">
			{#each Array(6) as _, starIndex (starIndex)}
				<span class="star">
					<svg viewBox="0 0 10 10"><path d="M5 0 L6 4 L10 5 L6 6 L5 10 L4 6 L0 5 L4 4 Z" fill="currentColor" /></svg>
				</span>
			{/each}
		</div>

		<!-- Animated book loading spinner -->
		<div class="book">
			<svg viewBox="0 0 140 110" xmlns="http://www.w3.org/2000/svg">
				<defs>
					<linearGradient id="sl-pageL" x1="0" y1="0" x2="1" y2="1">
						<stop offset="0" stop-color="#FBF5E9" />
						<stop offset="1" stop-color="#ECDCBE" />
					</linearGradient>
					<linearGradient id="sl-pageR" x1="1" y1="0" x2="0" y2="1">
						<stop offset="0" stop-color="#FBF5E9" />
						<stop offset="1" stop-color="#ECDCBE" />
					</linearGradient>
					<linearGradient id="sl-cover" x1="0" y1="0" x2="0" y2="1">
						<stop offset="0" stop-color="#7A4A2B" />
						<stop offset="1" stop-color="#5E3720" />
					</linearGradient>
					<linearGradient id="sl-sweep" x1="0" y1="0" x2="1" y2="0">
						<stop offset="0" stop-color="#fff" stop-opacity="0" />
						<stop offset="0.5" stop-color="#fff" stop-opacity="0.65" />
						<stop offset="1" stop-color="#fff" stop-opacity="0" />
						<animate attributeName="x1" values="-1;1" dur="3.2s" repeatCount="indefinite" />
						<animate attributeName="x2" values="0;2" dur="3.2s" repeatCount="indefinite" />
					</linearGradient>
					<clipPath id="sl-pages">
						<path d="M70 18 C52 8 26 8 10 16 L10 92 C26 84 52 84 70 94 Z" />
						<path d="M70 18 C88 8 114 8 130 16 L130 92 C114 84 88 84 70 94 Z" />
					</clipPath>
				</defs>

				<!-- Book cover below pages -->
				<path
					d="M70 96 C50 84 24 84 6 92 L6 98 C24 90 50 90 70 100 C90 90 116 90 134 98 L134 92 C116 84 90 84 70 96 Z"
					fill="url(#sl-cover)"
				/>

				<!-- Left + Right page -->
				<path d="M70 18 C52 8 26 8 10 16 L10 92 C26 84 52 84 70 94 Z" fill="url(#sl-pageL)" stroke="#C9B388" stroke-width="1" />
				<path d="M70 18 C88 8 114 8 130 16 L130 92 C114 84 88 84 70 94 Z" fill="url(#sl-pageR)" stroke="#C9B388" stroke-width="1" />

				<!-- Lines of text on pages -->
				<g stroke="#C9B388" stroke-width="1.4" stroke-linecap="round" opacity="0.8">
					<line x1="20" y1="30" x2="58" y2="26" />
					<line x1="20" y1="40" x2="58" y2="36" />
					<line x1="20" y1="50" x2="58" y2="46" />
					<line x1="20" y1="60" x2="58" y2="56" />
					<line x1="82" y1="26" x2="120" y2="30" />
					<line x1="82" y1="36" x2="120" y2="40" />
					<line x1="82" y1="46" x2="120" y2="50" />
					<line x1="82" y1="56" x2="120" y2="60" />
				</g>
				<path d="M70 18 L70 94" stroke="#B79A6A" stroke-width="2" opacity="0.5" />

				<!-- Bookmark ribbon -->
				<path d="M70 18 L70 70 L66 62 L62 70 L62 18 Z" fill="#B5482F" opacity="0.92" />

				<!-- Glow effect across pages -->
				<rect x="0" y="0" width="140" height="110" fill="url(#sl-sweep)" clip-path="url(#sl-pages)" />
			</svg>
		</div>

		<!-- Sparkles from pages -->
		<span class="sparkle s1"><svg viewBox="0 0 10 10"><path d="M5 0 L6 4 L10 5 L6 6 L5 10 L4 6 L0 5 L4 4 Z" fill="currentColor" /></svg></span>
		<span class="sparkle s2"><svg viewBox="0 0 10 10"><path d="M5 0 L6 4 L10 5 L6 6 L5 10 L4 6 L0 5 L4 4 Z" fill="currentColor" /></svg></span>
		<span class="sparkle s3"><svg viewBox="0 0 10 10"><path d="M5 0 L6 4 L10 5 L6 6 L5 10 L4 6 L0 5 L4 4 Z" fill="currentColor" /></svg></span>
		<span class="sparkle s4"><svg viewBox="0 0 10 10"><path d="M5 0 L6 4 L10 5 L6 6 L5 10 L4 6 L0 5 L4 4 Z" fill="currentColor" /></svg></span>
	</div>

	<!-- Rotating text lines + spinner subtitle -->
	<p class="line" class:swap={swapping} aria-hidden="true">{line}</p>
	{#if subtitle}
		<p class="subtle" role="status">{subtitle}</p>
	{/if}
</div>

<style>
    .loader {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 1.75rem;
        padding: 2rem;
        font-family: var(--font-story, 'Iowan Old Style', 'Palatino Linotype', Georgia, serif);
    }

    .stage {
        position: relative;
        width: 200px;
        height: 180px;
        display: grid;
        place-items: center;
    }

    .glow {
        position: absolute;
        top: 50%;
        left: 50%;
        width: 180px;
        height: 180px;
        margin: -90px 0 0 -90px;
        border-radius: 50%;
        background: radial-gradient(
                circle,
                rgba(217, 164, 65, 0.55) 0%,
                rgba(217, 164, 65, 0.18) 40%,
                transparent 70%
        );
        filter: blur(2px);
        animation: sl-glow 3.2s ease-in-out infinite;
    }
    @keyframes sl-glow {
        0%, 100% { transform: scale(0.92); opacity: 0.75; }
        50% { transform: scale(1.08); opacity: 1; }
    }

    .orbit {
        position: absolute;
        top: 50%;
        left: 50%;
        width: 188px;
        height: 188px;
        margin: -94px 0 0 -94px;
        animation: sl-spin 9s linear infinite;
    }
    .orbit .star {
        position: absolute;
        top: 50%;
        left: 50%;
        width: 7px;
        height: 7px;
        margin: -3.5px 0 0 -3.5px;
        color: var(--gold, #d9a441);
    }
    .orbit .star svg { display: block; width: 100%; height: 100%; }
    .orbit .star:nth-child(1) { transform: rotate(0deg) translateY(-94px); }
    .orbit .star:nth-child(2) { transform: rotate(60deg) translateY(-94px); }
    .orbit .star:nth-child(3) { transform: rotate(120deg) translateY(-94px); }
    .orbit .star:nth-child(4) { transform: rotate(180deg) translateY(-94px); }
    .orbit .star:nth-child(5) { transform: rotate(240deg) translateY(-94px); }
    .orbit .star:nth-child(6) { transform: rotate(300deg) translateY(-94px); }
    .orbit .star:nth-child(even) {
        color: var(--accent-cool, #7e9cb0);
        width: 5px;
        height: 5px;
        opacity: 0.85;
    }

    @keyframes sl-spin { to { transform: rotate(360deg); } }

    .book {
        position: relative;
        width: 130px;
        animation: sl-bob 3.2s ease-in-out infinite;
        filter: drop-shadow(0 8px 10px rgba(94, 55, 32, 0.28));
    }
    .book svg { display: block; width: 100%; height: auto; }
    @keyframes sl-bob {
        0%, 100% { transform: translateY(2px) rotate(-0.6deg); }
        50% { transform: translateY(-4px) rotate(0.6deg); }
    }

    .sparkle {
        position: absolute;
        color: var(--gold, #d9a441);
        opacity: 0;
        animation: sl-rise 3.6s ease-in-out infinite;
    }
    .sparkle svg { display: block; width: 100%; height: 100%; }
    .sparkle.s1 { left: 36px; bottom: 96px; width: 14px; height: 14px; animation-delay: 0s; }
    .sparkle.s2 { right: 30px; bottom: 104px; width: 11px; height: 11px; color: var(--accent-cool, #7e9cb0); animation-delay: 0.9s; }
    .sparkle.s3 { left: 64px; bottom: 120px; width: 9px; height: 9px; animation-delay: 1.7s; }
    .sparkle.s4 { right: 52px; bottom: 92px; width: 8px; height: 8px; color: var(--accent-warm, #b5482f); animation-delay: 2.4s; }
    @keyframes sl-rise {
        0% { opacity: 0; transform: translateY(8px) scale(0.4) rotate(0deg); }
        25% { opacity: 1; transform: translateY(-2px) scale(1) rotate(20deg); }
        60% { opacity: 0.9; transform: translateY(-16px) scale(0.9) rotate(-10deg); }
        100% { opacity: 0; transform: translateY(-30px) scale(0.5) rotate(15deg); }
    }

    .line {
        min-height: 1.6em;
        max-width: 22rem;
        margin: 0;
        text-align: center;
        font-size: 1.15rem;
        color: var(--text-muted, #6b5d4a);
        letter-spacing: 0.2px;
        transition: opacity 0.5s ease;
    }
    .line.swap { opacity: 0; }

    .subtle {
        margin: -0.6rem 0 0;
        font-size: 0.85rem;
        color: rgba(107, 93, 74, 0.7);
        font-style: italic;
    }

    @media (prefers-reduced-motion: reduce) {
        .glow, .orbit, .book, .sparkle { animation: none; }
        .sparkle { opacity: 0.6; }
    }
</style>
