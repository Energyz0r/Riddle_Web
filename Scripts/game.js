(function () {
    var CHARACTERS = [
        { key: 'farmer', label: 'Farmer' },
        { key: 'fox', label: 'Fox' },
        { key: 'chicken', label: 'Chicken' },
        { key: 'corn', label: 'Corn' }
    ];

    var urls = window.gameUrls;
    var state = null;     // { farmer, fox, chicken, corn } -> "East" | "West"
    var onBoat = null;    // passenger currently on the boat: null | "fox" | "chicken" | "corn"
    var moves = 0;
    var over = false;

    function $(id) { return document.getElementById(id); }

    function label(key) {
        for (var i = 0; i < CHARACTERS.length; i++)
            if (CHARACTERS[i].key === key) return CHARACTERS[i].label;
        return key;
    }

    function makeImg(key, faded) {
        var img = document.createElement('img');
        img.src = urls.images + key + (faded ? '_ph' : '') + '.png';
        img.alt = label(key) + (faded ? ' (not here)' : '');
        return img;
    }

    function setMessage(text, status) {
        var m = $('message');
        m.textContent = text;
        m.className = status || '';
    }

    function render() {
        // --- banks ---
        ['East', 'West'].forEach(function (side) {
            var slots = document.querySelector('#bank-' + side + ' .slots');
            slots.innerHTML = '';

            CHARACTERS.forEach(function (c) {
                var isFarmer = c.key === 'farmer';
                // The farmer is always on the boat, so his bank pictures are always faded.
                // Others are solid only on their own bank and only while not on the boat.
                var present = !isFarmer && state[c.key] === side && onBoat !== c.key;

                var slot = document.createElement('div');
                slot.className = 'slot';
                slot.appendChild(makeImg(c.key, !present));

                var boatIsHere = state.farmer === side;
                if (present && boatIsHere && !over) {
                    slot.classList.add('clickable');
                    slot.title = 'Click to put the ' + c.label.toLowerCase() + ' on the boat';
                    slot.onclick = function () { load(c.key); };
                }
                slots.appendChild(slot);
            });
        });

        // --- boat: sits next to the farmer's bank, farmer always in slot 1 ---
        $('boat').className = 'boat ' + (state.farmer === 'East' ? 'at-east' : 'at-west');

        var slotFarmer = $('slotFarmer');
        slotFarmer.innerHTML = '';
        slotFarmer.appendChild(makeImg('farmer', false));

        var slotPassenger = $('slotPassenger');
        slotPassenger.innerHTML = '';
        slotPassenger.className = 'boat-slot';
        slotPassenger.onclick = null;
        slotPassenger.title = '';
        if (onBoat) {
            slotPassenger.appendChild(makeImg(onBoat, false));
            if (!over) {
                slotPassenger.classList.add('clickable');
                slotPassenger.title = 'Click to take the ' + label(onBoat).toLowerCase() + ' off the boat';
                slotPassenger.onclick = unload;
            }
        }

        $('rowBtn').disabled = over;
        $('meta').textContent = 'Moves: ' + moves;
    }

    function load(key) {
        if (over) return;
        onBoat = key;
        setMessage('The ' + label(key).toLowerCase() + ' is on the boat. Press "Row across" to cross.', 'info');
        render();
    }

    function unload() {
        if (over || !onBoat) return;
        var k = onBoat;
        onBoat = null;
        setMessage('The ' + label(k).toLowerCase() + ' is back on the bank.', 'info');
        render();
    }

    function newGame() {
        fetch(urls.newGame, { credentials: 'same-origin' })
            .then(function (res) { return res.json(); })
            .then(function (r) {
                state = r.state; onBoat = null; moves = 0; over = false;
                setMessage(r.message, r.status);
                render();
            });
    }

    function row() {
        if (over) return;
        fetch(urls.move, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ state: state, passenger: onBoat })
        })
            .then(function (res) { return res.json(); })
            .then(function (r) {
                var crossed = r.status === 'valid' || r.status === 'won';
                if (crossed) {
                    moves++;
                    onBoat = null;      // boat is empty after every crossing
                }
                state = r.state;
                over = r.status === 'won';
                setMessage(r.message, r.status);
                render();
            });
    }

    $('rowBtn').addEventListener('click', row);
    $('resetBtn').addEventListener('click', newGame);
    newGame();
})();