        // PUT: api/Offer/5
        [HttpPut("[controller]/{id:int}")]
        public IActionResult<int?> Edit([FromRoute] int id, [FromBody] CreateUpdateOfferViewModel createUpdateOfferViewModel)
        {
            Offer offerToUpdate = offerService.GetById(id);
            if (offerToUpdate==null) return NotFound(id);
          
            offerToUpdate.Caption = createUpdateOfferViewModel.Caption;
            offerToUpdate.Description = createUpdateOfferViewModel.Description;
            offerToUpdate.ImgPath = createUpdateOfferViewModel.ImgPath;
            offerService.Update(offerToUpdate);

            return NoContent();
        }

        // DELETE: api/Offer/5
        [HttpDelete("[controller]/{id:int}")]
        public IActionResult<Offer?,int?> DeleteOffer([FromRoute] int id)
        {
            Offer offerToDelete = offerService.GetById(id);
            if (offerToDelete == null) return NotFound(id);

            offerService.Delete(offerToDelete);

            return Ok(offerToDelete);
        }


        // GET: api/Offer/5
        [HttpGet("[controller]/{id:int}")]
        public IActionResult<int?, OfferViewModel?> GetOffer([FromRoute] int id)
        {
            Offer offer = offerService.GetById(id);
            if (offer == null) return NotFound(id);

            PeopleFinderUser peopleFinderUser = userService.GetById(offer.CreatorId);
            OfferViewModel offerVM = new OfferViewModel
            {
                Id = offer.Id,
                ImgPath = offer.ImgPath,
                Caption = offer.Caption,
                Description = offer.Description,
                Timestamped = offer.Timestamped,
                Email = peopleFinderUser.Email,
                Telephone = peopleFinderUser.Telephone,
                FirstName = peopleFinderUser.FirstName,
                LastName = peopleFinderUser.LastName,
                Photo = bnetUrl + peopleFinderUser.Photo
            };
            
            return Ok(offerVM);
        }
